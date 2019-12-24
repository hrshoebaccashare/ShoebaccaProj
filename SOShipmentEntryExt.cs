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

        public PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentType, Equal<Required<SOOrderShipment.shipmentType>>,
            And<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>> OrderShipment;

        protected void SOShipment_RowPersisting(PXCache cache, PXRowPersistingEventArgs e, PXRowPersisting baseMethod)
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
                    if (orderExt.UsrISPrimeOrder == true)
                    {
                        shipmentExt.UsrISPrimeOrder = true;
                    }

                    if (shipmentExt.UsrDeliverByDate == null || orderExt.UsrDeliverByDate < shipmentExt.UsrDeliverByDate)
                    {
                        shipmentExt.UsrDeliverByDate = orderExt.UsrDeliverByDate;
                    }

                    if (orderExt.UsrGuaranteedDelivery == true)
                    {
                        shipmentExt.UsrGuaranteedDelivery = true;
                    }
                }
            }
        }

        protected void SOShipment_PickListPrinted_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var shipment = (SOShipment)e.Row;
            if (shipment.PickListPrinted == true)
            {
                sender.SetValueExt<SOShipmentExt.usrPickListPrintedDate>(shipment, DateTime.Now);
            }
            else
            {
                sender.SetValueExt<SOShipmentExt.usrPickListPrintedDate>(shipment, null);
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
                    shiporder = Base.Document.Current;

                    SOPackageDetail p = Base.Packages.SelectSingle();
                    if (p == null) throw new PXException(PX.Objects.SO.Messages.PackageIsRequired);

                    SelectLeastExpensiveShipVia();
                }
                else
                {
                    PXTrace.WriteInformation("Skipping rate shopping because a Ship Via is already selected.");
                }
            }

            var sw = new Stopwatch();
            sw.Start();
            baseMethod(shiporder);
            sw.Stop();
            PXTrace.WriteInformation($"ShipPackages took {sw.ElapsedMilliseconds}ms.");
        }

        [PXOverride]
        public void ConfirmShipment(SOOrderEntry docgraph, SOShipment shiporder, Action<SOOrderEntry, SOShipment> baseMethod)
        {
            baseMethod(docgraph, shiporder);

            SOPackageDetail TNbr = FirstTrackingNumber.Select(shiporder.ShipmentNbr);
            if (TNbr != null)
            {
                SOOrderShipment ShipNbr = OrderShipment.Select(shiporder.ShipmentType, shiporder.ShipmentNbr);
                if (ShipNbr != null)
                {
                    ShipNbr.GetExtension<SOOrderShipmentExt>().UsrShipmentTrackingNbr = TNbr.TrackNumber;
                    OrderShipment.Cache.Update(ShipNbr);
                    Base.Save.Press();
                }
            }
        }

        private void SelectLeastExpensiveShipVia()
        {
            var sw = new Stopwatch();
            sw.Start();
            PXTrace.WriteInformation("Starting rate shopping.");

            var shipment = Base.Document.Current;
            var shipmentExt = shipment.GetExtension<SOShipmentExt>();

            List<CarrierRequestInfo> requests = new List<CarrierRequestInfo>();
            var plugins = GetCarrierPluginsForAutoRateShopping();

            var carrierRatesExt = Base.GetExtension<SOShipmentEntry.CarrierRates>();
            MethodInfo buildQuoteRequestMethod = carrierRatesExt.GetType().GetMethod("BuildQuoteRequest", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public); //Unfortunately not exposed by SOShipmentEntry.CarrierRates...

            foreach (CarrierPlugin plugin in plugins)
            {
                //Skip if carrier plugin is specific to a site
                if (plugin.SiteID != null && plugin.SiteID != shipment.SiteID) continue;
                if (shipmentExt.UsrISPrimeOrder.GetValueOrDefault() != plugin.GetExtension<CarrierPluginExt>().UsrUseForPrimeOrders.GetValueOrDefault()) continue;

                ICarrierService cs = CarrierPluginMaint.CreateCarrierService(Base, plugin);
                CarrierRequest cr = (CarrierRequest)buildQuoteRequestMethod.Invoke(carrierRatesExt, new object[] { carrierRatesExt.Documents.Cache.GetExtension<PX.Objects.SO.GraphExtensions.CarrierRates.Document>(shipment), plugin });

                requests.Add(new CarrierRequestInfo
                {
                    Plugin = plugin,
                    SiteID = shipment.SiteID.Value,
                    Service = cs,
                    Request = cr
                });
            }

            Parallel.ForEach(requests, info => info.Result = info.Service.GetRateList(info.Request));

            string leastExpensiveCarrier = null;
            string leastExpensiveMethod = null;
            DateTime? leastExpensiveShipViaDeliveryDate = null;
            decimal? amount = null;

            //We're comparing with a date/time value returned by the carrier; anything delivered on the UsrDeliverByDate meets the SLA
            DateTime? deliverByDateTime = shipmentExt.UsrDeliverByDate == null ? null : new DateTime?(shipmentExt.UsrDeliverByDate.Value.Date.Add(new TimeSpan(23, 59, 59)));

            foreach (var request in requests)
            {
                if (!request.Result.IsSuccess) continue;
                foreach (var rate in request.Result.Result)
                {
                    string traceMessage = "Site: " + request.SiteID + " Carrier:" + request.Plugin.Description + " Method: " + rate.Method + " Delivery Date: " + (rate.DeliveryDate == null ? "" : rate.DeliveryDate.ToString()) + " Amount: " + rate.Amount.ToString();

                    if (rate.IsSuccess)
                    {
                        if (shipmentExt.UsrGuaranteedDelivery.GetValueOrDefault() == false || deliverByDateTime >= rate.DeliveryDate)
                        {
                            if (amount == null || rate.Amount < amount || (rate.Amount == amount && rate.DeliveryDate < leastExpensiveShipViaDeliveryDate))
                            {
                                leastExpensiveShipViaDeliveryDate = rate.DeliveryDate;
                                leastExpensiveCarrier = request.Plugin.CarrierPluginID;
                                leastExpensiveMethod = rate.Method.Code;
                                amount = rate.Amount;
                            }
                        }
                        else
                        {
                            traceMessage += " [TOO LATE]";
                        }
                    }
                    else
                    {
                        if (rate.Messages.Count > 0)
                        {
                            traceMessage += " [FAILED: " + rate.Messages[0].Description + "]";
                        }
                        else
                        {
                            traceMessage += " [FAILED WITH NO ERROR MESSAGE]";
                        }
                    }

                    PXTrace.WriteInformation(traceMessage);
                }
            }

            if (leastExpensiveCarrier == null)
            {
                throw new PXException(Messages.FailedToFindCarrierAndMethod);
            }
            else
            {
                sw.Stop();
                PXTrace.WriteInformation($"Rate shopping completed in {sw.ElapsedMilliseconds}ms. Least expensive carrier: {leastExpensiveCarrier} method: {leastExpensiveMethod} amount: {amount}");

                var carrier = (Carrier)PXSelectReadonly<Carrier,
                    Where<Carrier.carrierPluginID, Equal<Required<Carrier.carrierPluginID>>,
                    And<Carrier.pluginMethod, Equal<Required<Carrier.pluginMethod>>,
                    And<Carrier.isExternal, Equal<True>>>>>.Select(Base, leastExpensiveCarrier, leastExpensiveMethod);

                if (carrier == null)
                {
                    throw new PXException(Messages.NoShipViaFound, leastExpensiveCarrier, leastExpensiveMethod);
                }
                else
                {
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