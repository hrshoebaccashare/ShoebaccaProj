using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.AP;
using PX.Objects.SO;
using PX.Objects.CS;
using static ShoebaccaProj.PCBConst;
using PX.CarrierService;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ShoebaccaProj
{
    public class SOOrderEntryExt : PXGraphExtension<SOOrderEntry>
    {
        [PXButton, PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXOverride]
        public virtual IEnumerable Action(
         PXAdapter adapter,
         [PXInt]
        [PXIntList(new int[] { 1, 2, 3, 4, 5 }, new string[] { "Create Shipment", "Apply Assignment Rules", "Create Invoice", "Post Invoice to IN", "Create Purchase Order" })]
        int? actionID,
         [PXDate]
        DateTime? shipDate,
         [PXSelector(typeof(INSite.siteCD))]
        string siteCD,
         [SOOperation.List]
        string operation,
         [PXString()]
        string ActionName,
         Func<PXAdapter, int?, DateTime?, string, string, string, IEnumerable> baseMtd)
        {
            switch (actionID)
            {
                case 1:
                    {
                        if (!string.IsNullOrEmpty(ActionName))
                        {
                            PXAction action = Base.Actions[ActionName];

                            if (action != null)
                            {
                                Base.Save.Press();
                                List<object> result = new List<object>();
                                foreach (object data in action.Press(adapter))
                                {
                                    result.Add(data);
                                }
                                return result;
                            }
                        }

                        List<SOOrder> list = new List<SOOrder>();
                        foreach (SOOrder order in adapter.Get<SOOrder>())
                        {
                            list.Add(order);
                        }

                        if (shipDate != null)
                        {
                            Base.soparamfilter.Current.ShipDate = shipDate;
                        }

                        if (Base.soparamfilter.Current.ShipDate == null)
                        {
                            Base.soparamfilter.Current.ShipDate = Base.Accessinfo.BusinessDate;
                        }

                        if (siteCD != null)
                        {
                            Base.soparamfilter.Cache.SetValueExt<SOParamFilter.siteID>(Base.soparamfilter.Current, siteCD);
                        }

                        if (!adapter.MassProcess)
                        {
                            if (Base.soparamfilter.Current.SiteID == null)
                            {
                                Base.soparamfilter.Current.SiteID = GetPreferedSiteID();
                            }
                            if (adapter.ExternalCall)
                                Base.soparamfilter.AskExt(true);
                        }
                        if (Base.soparamfilter.Current.SiteID != null || adapter.MassProcess)
                        {
                            try
                            {
                                Base.RecalculateExternalTaxesSync = true;
                                Base.Save.Press();
                            }
                            finally
                            {
                                Base.RecalculateExternalTaxesSync = false;
                            }
                            PXAutomation.RemovePersisted(Base, typeof(SOOrder), new List<object>(list));

                            SOParamFilter filter = Base.soparamfilter.Current;
                            PXLongOperation.StartOperation(Base, delegate ()
                            {
                                bool anyfailed = false;
                                SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
                                SOOrderEntry ordgraph = PXGraph.CreateInstance<SOOrderEntry>();
                                DocumentList<SOShipment> created = new DocumentList<SOShipment>(docgraph);

                                //address AC-92776
                                for (int i = 0; i < list.Count; i++)
                                {
                                    SOOrder order = list[i];
                                    if (adapter.MassProcess)
                                    {
                                        PXProcessing<SOOrder>.SetCurrentItem(order);
                                    }

                                    SOOrder ordercopy = (SOOrder)Base.Caches[typeof(SOOrder)].CreateCopy(order);
                                    try
                                    {
                                        if(operation == SOOperation.Issue)
                                        { 
                                            ReviewWarehouseAvailability(ordgraph, order);
                                        }
                                    }
                                    catch(SOShipmentException ex)
                                    {
                                        Base.Caches[typeof(SOOrder)].RestoreCopy(order, ordercopy);
                                        if (!adapter.MassProcess)
                                        {
                                            throw;
                                        }

                                        order.LastShipDate = filter.ShipDate;
                                        order.Status = SOOrderStatus.Shipping; //Automation will set the order to back order since no shipments were created

                                        docgraph.Clear();

                                        var ordergraph = PXGraph.CreateInstance<SOOrderEntry>();
                                        ordergraph.Clear();

                                        ordergraph.Document.Cache.MarkUpdated(order);
                                        PXAutomation.CompleteSimple(ordergraph.Document.View);
                                        try
                                        {
                                            ordergraph.Save.Press();
                                            PXAutomation.RemovePersisted(ordergraph, order);

                                            PXTrace.WriteInformation(ex);
                                            PXProcessing<SOOrder>.SetWarning(ex);
                                        }
                                        catch (Exception inner)
                                        {
                                            Base.Caches[typeof(SOOrder)].RestoreCopy(order, ordercopy);
                                            PXProcessing<SOOrder>.SetError(inner);
                                            anyfailed = true;
                                        }
                                        continue; //Stop there for this order
                                    }
                                    catch (Exception ex)
                                    {
                                        if (!adapter.MassProcess)
                                        {
                                            throw;
                                        }
                                        PXProcessing<SOOrder>.SetError(ex);
                                        anyfailed = true;
                                        continue;
                                    }

                                    List<int?> sites = new List<int?>();

                                    if (filter.SiteID != null)
                                    {
                                        sites.Add(filter.SiteID);
                                    }
                                    else
                                    {
                                        foreach (SOShipmentPlan plan in PXSelectGroupBy<SOShipmentPlan, Where<SOShipmentPlan.orderType, Equal<Current<SOOrder.orderType>>, And<SOShipmentPlan.orderNbr, Equal<Current<SOOrder.orderNbr>>>>, Aggregate<GroupBy<SOShipmentPlan.siteID>>, OrderBy<Asc<SOShipmentPlan.siteID>>>.SelectMultiBound(docgraph, new object[] { order }))
                                        {
                                            sites.Add(plan.SiteID);
                                        }
                                    }

                                    foreach (int? SiteID in sites)
                                    {
                                        ordercopy = (SOOrder)Base.Caches[typeof(SOOrder)].CreateCopy(order);
                                        try
                                        {
                                            using (var ts = new PXTransactionScope())
                                            {
                                                PXTimeStampScope.SetRecordComesFirst(typeof(SOOrder), true);
                                                docgraph.CreateShipment(order, SiteID, filter.ShipDate, adapter.MassProcess, operation, created, adapter.QuickProcessFlow);
                                                ts.Complete();
                                            }

                                            if (adapter.MassProcess)
                                            {
                                                PXProcessing<SOOrder>.SetProcessed();
                                            }
                                        }
                                        catch (SOShipmentException ex)
                                        {
                                            Base.Caches[typeof(SOOrder)].RestoreCopy(order, ordercopy);
                                            if (!adapter.MassProcess)
                                            {
                                                throw;
                                            }
                                            order.LastSiteID = SiteID;
                                            order.LastShipDate = filter.ShipDate;
                                            order.Status = SOOrderStatus.Shipping;

                                            docgraph.Clear();

                                            var ordergraph = PXGraph.CreateInstance<SOOrderEntry>();
                                            ordergraph.Clear();

                                            ordergraph.Document.Cache.MarkUpdated(order);
                                            PXAutomation.CompleteSimple(ordergraph.Document.View);
                                            try
                                            {
                                                ordergraph.Save.Press();
                                                PXAutomation.RemovePersisted(ordergraph, order);

                                                PXTrace.WriteInformation(ex);
                                                PXProcessing<SOOrder>.SetWarning(ex);
                                            }
                                            catch (Exception inner)
                                            {
                                                Base.Caches[typeof(SOOrder)].RestoreCopy(order, ordercopy);
                                                PXProcessing<SOOrder>.SetError(inner);
                                                anyfailed = true;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Base.Caches[typeof(SOOrder)].RestoreCopy(order, ordercopy);
                                            docgraph.Clear();

                                            if (!adapter.MassProcess)
                                            {
                                                throw;
                                            }
                                            PXProcessing<SOOrder>.SetError(ex);
                                            anyfailed = true;
                                        }
                                    }
                                }

                                if (adapter.AllowRedirect && !adapter.MassProcess && created.Count > 0)
                                {
                                    using (new PXTimeStampScope(null))
                                    {
                                        docgraph.Clear();
                                        docgraph.Document.Current = docgraph.Document.Search<SOShipment.shipmentNbr>(created[0].ShipmentNbr);
                                        throw new PXRedirectRequiredException(docgraph, "Shipment");
                                    }
                                }

                                if (anyfailed)
                                {
                                    throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
                                }
                            });
                        }
                        return list;
                    }
                default:
                    return baseMtd(adapter, actionID, shipDate, siteCD, operation, ActionName);
            }
        }

        private void ReviewWarehouseAvailability(SOOrderEntry orderEntryGraph, SOOrder order)
        {
            //Exclude orders that have a preferred warehouse set (ex: NAFTA warehouse orders)
            if (order.DefaultSiteID == null)
            {
                PXTrace.WriteInformation("*Reviewing warehouse availability for order " + order.OrderType + " " + order.OrderNbr);

                orderEntryGraph.Clear();
                orderEntryGraph.Document.Current = orderEntryGraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);

                var sortedSiteList = new List<int?>();
                var availabilityInfo = new Dictionary<Tuple<int?, int?>, decimal>();

                var sitesThatCanShipComplete = new List<int>();
                foreach (INSite site in PXSelect<INSite, Where<INSite.active, Equal<True>, And<INSiteExt.usrAllowShippingAutoSelection, Equal<True>>>, OrderBy<Desc<INSite.siteCD>>>.Select(Base))
                {
                    sortedSiteList.Add(site.SiteID);
                    bool canShipComplete = true;

                    // Check if current warehouse can fulfill the order *completely*
                    foreach (PXResult<SOLine, INSiteStatus> res in PXSelectJoin<SOLine,
                        LeftJoin<INSiteStatus, On<SOLine.inventoryID, Equal<INSiteStatus.inventoryID>,
                            And<SOLine.subItemID, Equal<INSiteStatus.subItemID>,
                            And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>>,
                        Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
                            And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>,
                            And<SOLine.operation, Equal<Required<SOLine.operation>>>>>>.Select(orderEntryGraph, site.SiteID, SOOperation.Issue))
                    {
                        SOLine line = (SOLine)res;
                        INSiteStatus ss = (INSiteStatus)res;

                        //We cache availability info for later use, in case we're unable to completely fulfill order 
                        //from this site we'll fulfill it from closest site.
                        decimal qtyHardAvail = ss == null ? 0 : ss.QtyHardAvail.GetValueOrDefault();
                        var key = new Tuple<int?, int?>(line.InventoryID, site.SiteID);
                        if (!availabilityInfo.ContainsKey(key)) //Same Item/Site may be on the order more than once
                        {
                            availabilityInfo.Add(key, qtyHardAvail);
                        }

                        if (line.BaseOpenQty > qtyHardAvail && line.Completed.GetValueOrDefault() == false && line.IsStockItem == true)
                        {
                            PXTrace.WriteInformation("Not enough inventory available for item " + line.TranDesc + " in warehouse " + site.SiteCD + " (QtyHardAvail: " + qtyHardAvail + ")");
                            canShipComplete = false;
                        }
                    }

                    if (canShipComplete)
                    {
                        sitesThatCanShipComplete.Add(site.SiteID.Value);
                    }
                }

                if (sitesThatCanShipComplete.Count == 0)
                {
                    //We haven't found a single warehouse that can fulfill this order completely. Assign based on availability.
                    PXTrace.WriteInformation("No warehouse can fulfill order " + order.OrderNbr + " completely. Assigning on a per-item basis.");
                    int? createShipmentSiteID = null;
                    foreach (SOLine line in orderEntryGraph.Transactions.Select())
                    {
                        if (line.Operation == SOOperation.Issue && line.Completed.GetValueOrDefault() == false && line.IsStockItem == true)
                        {
                            bool foundSiteWithAvailability = false;
                            foreach (int? currentSiteId in sortedSiteList)
                            {
                                decimal qtyHardAvail;
                                if (availabilityInfo.TryGetValue(new Tuple<int?, int?>(line.InventoryID, currentSiteId), out qtyHardAvail))
                                {
                                    if (qtyHardAvail >= line.BaseOpenQty)
                                    {
                                        if (createShipmentSiteID == null)
                                        {
                                            createShipmentSiteID = currentSiteId;
                                        }

                                        if (line.SiteID != currentSiteId)
                                        {
                                            line.SiteID = currentSiteId;
                                            orderEntryGraph.Transactions.Update(line);
                                        }

                                        foundSiteWithAvailability = true;
                                        break;
                                    }
                                }
                            }

                            if(!foundSiteWithAvailability)
                            {
                                //Dropship if possible -- we only dropship if quantity available at vendor exceeds demand for this order
                                decimal? qtyDropshippable = GetQuantityAvailableForDropship(line.InventoryID, line.SubItemID, line.SiteID);
                                PXTrace.WriteInformation("Item " + line.TranDesc + " can't be fulfilled from any warehouse. Quantity available for dropshipping: " + qtyDropshippable);
                                if (qtyDropshippable >= line.BaseOpenQty)
                                {
                                    line.POCreate = true;
                                    line.POSource = INReplenishmentSource.DropShipToOrder;
                                    orderEntryGraph.Transactions.Update(line);
                                }
                                else if (order.ShipComplete == SOShipComplete.ShipComplete)
                                {
                                    //If order is set to ShipComplete, we don't want to make ANY shipment to give the opportunity for a manual review of the order (and potentially replenish from NAFTA warehouse...)
                                    throw new SOShipmentException("Order can't be dropshipped or shipped in full.");
                                }
                            }
                        }
                    }
                }
                else if(sitesThatCanShipComplete.Count == 1)
                {
                    //TODO: Do a rate check, and pick warehouse based on 
                    PXTrace.WriteInformation("Warehouse " + sitesThatCanShipComplete[0] + " can fulfill order completely");
                    AssignSiteToOpenStockItems(orderEntryGraph, sitesThatCanShipComplete[0]);
                }
                else
                {
                    PXTrace.WriteInformation("Multiple warehouses can fulfill order completely -- do a rate check");

                    if (Base.Document.Current.IsManualPackage == true)
                    {
                        PXTrace.WriteInformation("Order is set for manual packaging. Automated Rate Shopping can't be done; selecting warehouse used the packages.");
                        AssignSiteToOpenStockItems(orderEntryGraph, sitesThatCanShipComplete[0]);
                    }
                    else
                    {
                        var sw = new Stopwatch();
                        sw.Start();

                        var orderExt = order.GetExtension<SOOrderExt>();
                        AssignSiteToOpenStockItems(orderEntryGraph, SelectLeastExpensiveShipWarehouse(sitesThatCanShipComplete, orderExt.UsrDeliverByDate, orderExt.UsrGuaranteedDelivery.GetValueOrDefault()));

                        sw.Stop();
                        PXTrace.WriteInformation($"Rate shopping took {sw.ElapsedMilliseconds}ms");
                    }
                }

                //We do not want to recalculate taxes here. We'll have recalculation and write off when Invoice is created
                //So we restore all the tax flags to their initial values
                orderEntryGraph.Document.Current.IsTaxValid = order.IsTaxValid;
                orderEntryGraph.Document.Current.IsOpenTaxValid = order.IsOpenTaxValid;
                orderEntryGraph.Document.Current.IsUnbilledTaxValid = order.IsUnbilledTaxValid;
                orderEntryGraph.Document.Current.IsFreightTaxValid = order.IsFreightTaxValid;
                
                //Save the order
                orderEntryGraph.Actions.PressSave();
            }
        }

        private int SelectLeastExpensiveShipWarehouse(IEnumerable<int> sites, DateTime? deliverBy, bool guaranteedDelivery)
        {
            var carrierRatesExt = Base.GetExtension<SOOrderEntry.CarrierRates>();
            var rateShoppingExt = Base.GetExtension<SOCarrierRateShoppingExt>();

            try
            {
                List<CarrierRequestInfo> requests = new List<CarrierRequestInfo>();
                rateShoppingExt.Active = true;

                var plugins = GetCarrierPluginsForAutoRateShopping();
                
                foreach (int siteID in sites)
                {
                    foreach (CarrierPlugin plugin in plugins)
                    {
                        //Skip if carrier plugin is specific to a site
                        if (plugin.SiteID != null && plugin.SiteID != siteID) continue;

                        rateShoppingExt.OverrideSiteID = siteID;
                        ICarrierService cs = CarrierPluginMaint.CreateCarrierService(Base, plugin);
                        CarrierRequest cr = carrierRatesExt.BuildQuoteRequest(Base.Document.Current, plugin);
                        
                        requests.Add(new CarrierRequestInfo
                        {
                            Plugin = plugin,
                            SiteID = siteID,
                            Service = cs,
                            Request = cr
                        });
                    }
                }

                Parallel.ForEach(requests, info => info.Result = info.Service.GetRateList(info.Request));

                int? leastExpensiveSiteID = null;
                decimal? amount = null;

                //We're comparing with a date/time value returned by the carrier; anything delivered on the UsrDeliverByDate meets the SLA
                DateTime? deliverByDateTime = deliverBy == null ? null : new DateTime?(deliverBy.Value.Date.Add(new TimeSpan(23, 59, 59)));

                Random rand = new Random();
                foreach (var request in requests)
                {
                    if (!request.Result.IsSuccess) continue;
                    foreach (var rate in request.Result.Result)
                    {
                        string traceMessage = "Site: " + request.SiteID + " Carrier:" + request.Plugin.Description + " Method: " + rate.Method + " Delivery Date: " + (rate.DeliveryDate == null ? "" : rate.DeliveryDate.ToString()) + " Amount: " + rate.Amount.ToString();

                        if (!rate.IsSuccess) continue;
                        if(guaranteedDelivery == false || deliverByDateTime >= rate.DeliveryDate)
                        {
                            if (amount == null || rate.Amount < amount)
                            {
                                leastExpensiveSiteID = request.SiteID;
                                amount = rate.Amount;
                            }
                            else if(amount == rate.Amount && request.SiteID != leastExpensiveSiteID)
                            {
                                PXTrace.WriteInformation("Same rate found for another site - decide if we use the other site based on random (could be based on actual workload by warehouse instead...)");
                                if (rand.NextDouble() >= 0.5)
                                {
                                    leastExpensiveSiteID = request.SiteID;
                                }
                            }
                        }
                        else
                        {
                            traceMessage += " [TOO LATE]";
                        }

                        PXTrace.WriteInformation(traceMessage);
                    }
                }

                if(leastExpensiveSiteID == null)
                {
                    throw new PXException(Messages.FailedToFindShipFromWarehouseAndCarrier);
                }
                else
                {
                    return leastExpensiveSiteID.Value;
                }
            }
            finally
            {
                rateShoppingExt.Active = false;
            }
        }

        private IEnumerable<CarrierPlugin> GetCarrierPluginsForAutoRateShopping()
        {
            return PXSelect<CarrierPlugin, Where<CarrierPluginExt.usrUseForAutoRateShopping, Equal<True>>>.Select(Base).RowCast<CarrierPlugin>();
        }

        private void AssignSiteToOpenStockItems(SOOrderEntry graph, int? siteID)
        {
            foreach (SOLine line in graph.Transactions.Select())
            {
                if (line.Operation == SOOperation.Issue && line.Completed.GetValueOrDefault() == false && line.IsStockItem == true)
                {
                    if (line.SiteID != siteID)
                    {
                        line.SiteID = siteID;
                        graph.Transactions.Update(line);
                    }
                }
            }
        }

        private Int32? GetPreferedSiteID()
        {
            int? siteID = null;
            PXResultset<SOOrderSite> osites = PXSelectJoin<SOOrderSite,
              InnerJoin<INSite, On<INSite.siteID, Equal<SOOrderSite.siteID>>>,
              Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>,
                And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>,
                  And<Match<INSite, Current<AccessInfo.userName>>>>>>.Select(Base);
            SOOrderSite preferred;
            if (osites.Count == 1)
            {
                siteID = ((SOOrderSite)osites).SiteID;
            }
            else if ((preferred = PXSelectJoin<SOOrderSite,
                  InnerJoin<INSite, On<INSite.siteID, Equal<SOOrderSite.siteID>>>,
                  Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>,
                    And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>,
                      And<SOOrderSite.siteID, Equal<Current<SOOrder.defaultSiteID>>,
                        And<Match<INSite, Current<AccessInfo.userName>>>>>>>.Select(Base)) != null)
            {
                siteID = preferred.SiteID;
            }
            return siteID;
        }
        
        private decimal? GetQuantityAvailableForDropship(int? inventoryID, int? subItemID, int? siteID)
        {
            var result = (PXResult<InventoryItem, CSAnswers>)SelectFrom<InventoryItem>
                .LeftJoin<CSAnswers>
                    .On<InventoryItem.noteID.IsEqual<CSAnswers.refNoteID>
                    .And<CSAnswers.attributeID.IsEqual<nodropshipAttribute>>>
                .Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>
                .View
                .ReadOnly
                .Select(Base, inventoryID);
            var item = (InventoryItem)result;
            InventoryItemExtn itemExt = PXCache<InventoryItem>.GetExtension<InventoryItemExtn>(item);
                 
            var attr = (CSAnswers)result;
            bool dropshipAllowed = (attr == null || attr.Value == "0");
            
            if (dropshipAllowed)
            {
                return itemExt.UsrQuantityatVendor;
            }
            else
            {
                return 0;
            }
        }

        protected void SOOrder_UsrOrderPlacedTimestamp_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            const string DefaultCalendarID = "SBWHSE";

            //Calculate expected ship date based on when the order was originally received (on shoebacca.com, Amazon, etc. -- not when it was pushed to Acumatica)
            var order = (SOOrder)e.Row;
            var orderExt = (SOOrderExt)sender.GetExtension<SOOrderExt>(order);
            if (orderExt.UsrOrderPlacedTimestamp != null)
            {
                sender.SetValue<SOOrder.shipDate>(order, CalendarHelper.GetClosestWarehouseDay(Base, DefaultCalendarID, orderExt.UsrOrderPlacedTimestamp.Value));
            }
        }        
    }
}
