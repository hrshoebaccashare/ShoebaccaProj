using System;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.PO;
using PX.TM;
using System.Collections.Generic;
using PX.CarrierService;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using PX.Objects.CM;

namespace ShoebaccaProj
{
	public class SOShipmentEntryExt : PXGraphExtension<SOShipmentEntry>
	{
		public PXSelect<SOPackageDetail,
			Where<SOPackageDetail.shipmentNbr, Equal<Required<SOPackageDetail.shipmentNbr>>>,
			OrderBy<Asc<SOPackageDetail.lineNbr>>> FirstTrackingNumber;

		public PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>> OrderShipment;

        [PXOverride]
        public void ShipPackages(SOShipment shiporder, Action<SOShipment> baseMethod)
        {
            if (shiporder.Operation == SOOperation.Issue)
            {
                if (String.IsNullOrEmpty(shiporder.ShipVia) || shiporder.ShipVia == "FREESHIP" || shiporder.ShipVia == "GROUND" || shiporder.ShipVia == "OVERNIGHT" || shiporder.ShipVia == "2DAY")
                {
                    Base.Clear();
                    Base.Document.Current = Base.Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
                    
                    SOPackageDetail p = Base.Packages.SelectSingle();
                    if (p == null) throw new PXException(PX.Objects.SO.Messages.PackageIsRequired);
                    
                    SelectLeastExpensiveShipVia(shiporder);
                }
                else
                {
                    PXTrace.WriteInformation("Skipping rate shopping because a Ship Via is already selected.");
                }
            }

            baseMethod(shiporder);
        }

        [PXOverride]
		public void ConfirmShipment(SOOrderEntry docgraph, SOShipment shiporder, Action<SOOrderEntry, SOShipment> baseMethod)
		{
            baseMethod(docgraph, shiporder);

			SOPackageDetail TNbr = FirstTrackingNumber.Select(shiporder.ShipmentNbr);
			if (TNbr != null)
			{
				SOOrderShipment ShipNbr = OrderShipment.Select(shiporder.ShipmentNbr);
				if (ShipNbr != null)
				{
					ShipNbr.GetExtension<SOOrderShipmentExt>().UsrShipmentTrackingNbr = TNbr.TrackNumber;
					OrderShipment.Cache.Update(ShipNbr);
					Base.Save.Press();
				}
			}
		}

        private void SelectLeastExpensiveShipVia(SOShipment shiporder)
        {
            PXTrace.WriteInformation("Starting rate shopping.");

            DateTime? deliverBy = null;
            bool guaranteedDelivery = false;
            bool isPrimeOrder = false;
            foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact> result in Base.OrderList.Select())
            {
                var order = (SOOrder)result;
                var orderExt = order.GetExtension<SOOrderExt>();
                if (orderExt.UsrDeliverByDate < deliverBy || deliverBy == null)
                {
                    //If there's more than one order linked to this shipment, we keep the earliest delivery by date
                    deliverBy = orderExt.UsrDeliverByDate;
                }

                if(orderExt.UsrGuaranteedDelivery == true)
                {
                    guaranteedDelivery = true;
                }
                
                //Note: the existing Kensium ABS customization has the UsrISPrimeOrder flag in the shipment too, and it's passed from the SO to the Shipment. We're not using it.
                if(orderExt.UsrISPrimeOrder == true)
                {
                    PXTrace.WriteInformation("This shipment is for a prime order.");
                    isPrimeOrder = true;
                }
            }

            List<CarrierRequestInfo> requests = new List<CarrierRequestInfo>();
            var plugins = GetCarrierPluginsForAutoRateShopping();

            var carrierRatesExt = Base.GetExtension<SOShipmentEntry.CarrierRates>();
            MethodInfo buildQuoteRequestMethod = carrierRatesExt.GetType().GetMethod("BuildQuoteRequest", BindingFlags.NonPublic | BindingFlags.Instance); //Unfortunately not exposed by SOShipmentEntry.CarrierRates...

            foreach (CarrierPlugin plugin in plugins)
            {
                //Skip if carrier plugin is specific to a site
                if (plugin.SiteID != null && plugin.SiteID != shiporder.SiteID) continue;
                if (isPrimeOrder != plugin.GetExtension<CarrierPluginExt>().UsrUseForPrimeOrders.GetValueOrDefault()) continue;
                
                ICarrierService cs = CarrierPluginMaint.CreateCarrierService(Base, plugin);
                CarrierRequest cr = (CarrierRequest) buildQuoteRequestMethod.Invoke(carrierRatesExt, new object[] { carrierRatesExt.Documents.Cache.GetExtension<PX.Objects.SO.GraphExtensions.CarrierRates.Document>(shiporder), plugin });

                requests.Add(new CarrierRequestInfo
                {
                    Plugin = plugin,
                    SiteID = shiporder.SiteID.Value,
                    Service = cs,
                    Request = cr
                });
            }

            Parallel.ForEach(requests, info => info.Result = info.Service.GetRateList(info.Request));

            string leastExpensiveCarrier = null;
            string leastExpensiveMethod = null;
            DateTime? leastExpensiveShipViaDeliveryDate = null;
            decimal? amount = null;
            
            foreach (var request in requests)
            {
                if (!request.Result.IsSuccess) continue;
                foreach (var rate in request.Result.Result)
                {
                    string traceMessage = "Site: " + request.SiteID + " Carrier:" + request.Plugin.Description + " Method: " + rate.Method + " Delivery Date: " + rate.DeliveryDate == null ? "" : rate.DeliveryDate.ToString() + " Amount: " + rate.Amount.ToString();

                    if (!rate.IsSuccess) continue;
                    if (guaranteedDelivery && deliverBy >= rate.DeliveryDate)
                    {
                        if (amount == null || rate.Amount < amount || (rate.Amount == amount && rate.DeliveryDate < leastExpensiveShipViaDeliveryDate))
                        {
                            leastExpensiveCarrier = request.Plugin.CarrierPluginID;
                            leastExpensiveMethod = rate.Method.Code;
                            amount = rate.Amount;
                        }
                    }
                    else
                    {
                        traceMessage += " [TOO LATE]";
                    }

                    PXTrace.WriteVerbose(traceMessage);
                }
            }

            if(leastExpensiveCarrier == null)
            {
                throw new PXException(Messages.FailedToFindCarrierAndMethod);
            }
            else
            {
                PXTrace.WriteInformation($"Least expensive carrier: {leastExpensiveCarrier} method: {leastExpensiveMethod} amount: {amount}");

                var carrier = (Carrier) PXSelectReadonly<Carrier,
                    Where<Carrier.carrierPluginID, Equal<Required<Carrier.carrierPluginID>>,
                    And<Carrier.pluginMethod, Equal<Required<Carrier.pluginMethod>>,
                    And<Carrier.isExternal, Equal<True>>>>>.Select(Base, leastExpensiveCarrier, leastExpensiveMethod);

                if(carrier == null)
                {
                    throw new PXException(Messages.NoShipViaFound, leastExpensiveCarrier, leastExpensiveMethod);
                }
                else
                {
                    //Set new ShipVia so that existing logic can pick it up
                    shiporder.ShipVia = carrier.CarrierID;

                    Base.Document.Current.ShipVia = carrier.CarrierID;
                    Base.Document.Update(Base.Document.Current);
                    Base.Document.Current.IsPackageValid = true; //Changing ShipVia will otherwise trigger package refresh
                    Base.Document.Update(Base.Document.Current);

                    Base.Save.Press();
                }
            }
        }

        private IEnumerable<CarrierPlugin> GetCarrierPluginsForAutoRateShopping()
        {
            return PXSelect<CarrierPlugin, Where<CarrierPluginExt.usrUseForAutoRateShopping, Equal<True>>>.Select(Base).RowCast<CarrierPlugin>();
        }
    }
}