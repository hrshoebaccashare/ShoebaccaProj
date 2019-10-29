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

        protected virtual void SOShipment_RowPersisting(PXCache cache, PXRowPersistingEventArgs e, PXRowPersisting baseMethod)
        {
            baseMethod(cache, e);

            SOShipment row = e.Row as SOShipment;
            if (row != null && e.Operation == PXDBOperation.Insert)
            {
                SOShipmentExt shipmentExt = row.GetExtension<SOShipmentExt>();               
                foreach (PXResult<SOOrderShipment, SOOrder> result in Base.OrderList.Select())
                {
                    var order = (SOOrder)result;
                    var orderExt = order.GetExtension<SOOrderExt>();

                    // As soon as we encounter an order marked Prime, we consider the shipment to be an Amazon Prime shipment.
                    if(orderExt.UsrISPrimeOrder == true)
                    {
                        shipmentExt.UsrISPrimeOrder = true;
                    }

                    if(shipmentExt.UsrDeliverByDate == null || orderExt.UsrDeliverByDate < shipmentExt.UsrDeliverByDate)
                    {
                        shipmentExt.UsrDeliverByDate = orderExt.UsrDeliverByDate;
                    }
                    
                    if(orderExt.UsrGuaranteedDelivery == true)
                    {
                        shipmentExt.UsrGuaranteedDelivery = true;
                    }
                }
            }
        }

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

            var shipmentExt = shiporder.GetExtension<SOShipmentExt>();

            List<CarrierRequestInfo> requests = new List<CarrierRequestInfo>();
            var plugins = GetCarrierPluginsForAutoRateShopping();

            var carrierRatesExt = Base.GetExtension<SOShipmentEntry.CarrierRates>();
            MethodInfo buildQuoteRequestMethod = carrierRatesExt.GetType().GetMethod("BuildQuoteRequest", BindingFlags.NonPublic | BindingFlags.Instance); //Unfortunately not exposed by SOShipmentEntry.CarrierRates...

            foreach (CarrierPlugin plugin in plugins)
            {
                //Skip if carrier plugin is specific to a site
                if (plugin.SiteID != null && plugin.SiteID != shiporder.SiteID) continue;
                if (shipmentExt.UsrISPrimeOrder.GetValueOrDefault() != plugin.GetExtension<CarrierPluginExt>().UsrUseForPrimeOrders.GetValueOrDefault()) continue;
                
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
                    if (shipmentExt.UsrGuaranteedDelivery.GetValueOrDefault() == false || shipmentExt.UsrDeliverByDate >= rate.DeliveryDate)
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