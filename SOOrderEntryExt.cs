using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.AP;
using PX.Objects.SO;
using PX.Objects.CS;

namespace PXDropShipPOExtPkg
{
    public class SOOrderEntryExt : PXGraphExtension<SOOrderEntry>
    {
        /// <summary>
        /// Dropship customization should work for only SB branch, so created constant for SB
        /// </summary>
        private const string BranchName = "SB";

        /// <summary>
        /// Getting the Available quantity from 1 to 98 and 99 location
        /// </summary>
        /// <param name="InventoryId">Inventory id</param>
        /// <param name="SubitemID">sub item id</param>
        /// <param name="SiteID">warehouse id</param>
        /// <param name="SOQty">soquantity</param>
        /// <param name="line">so line object</param>
        /// <returns>returns the availablequantity object with all details</returns>
        private AvailableQuantity CheckForAvailableQty(int InventoryId, int SubitemID, int SiteID, decimal SOQty, SOLine line)
        {
            AvailableQuantity output = new AvailableQuantity();
            try
            {

                InventorySummaryEnquiryResult result = new InventorySummaryEnquiryResult();

                decimal availSoQty = 0;
                InventorySummaryEnq newItemsumary = PXGraph.CreateInstance<InventorySummaryEnq>();
                InventorySummaryEnqFilter filter = newItemsumary.Filter.Current;
                filter.InventoryID = InventoryId;
                if (SubitemID > 0)
                {
                    PXSelectBase<INSubItem> subitem = new PXSelect<INSubItem, Where<INSubItem.subItemID, Equal<Required<INSubItem.subItemID>>>>(Base);
                    INSubItem sitem = subitem.Select(SubitemID);
                    if (sitem != null)
                    {
                        filter.SubItemCD = sitem.SubItemCD;
                    }
                }
                if (SiteID > 0)
                {
                    filter.SiteID = SiteID;
                }
                newItemsumary.Filter.Cache.Update(filter);
                PXSelectBase<INLocationStatus> PriorityLocations = new PXSelectJoin<INLocationStatus,
                    InnerJoin<INLocation, On<INLocation.locationID, Equal<INLocationStatus.locationID>>>,
                    Where<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>,
                    And<INLocationStatus.siteID, Equal<Required<INLocationStatus.inventoryID>>,
                    And<INLocationStatus.subItemID, Equal<Required<INLocationStatus.subItemID>>>>>>(Base);

                foreach (INLocationStatus item in PriorityLocations.Select(filter.InventoryID, filter.SiteID, SubitemID))
                {
                    filter.LocationID = item.LocationID;
                    newItemsumary.Filter.Cache.Update(filter);
                    foreach (InventorySummaryEnquiryResult detail in newItemsumary.ISERecords.Select())
                    {
                        if (detail.QtyAvail > 0)
                        {
                            availSoQty = availSoQty + detail.QtyAvail ?? 0;
                        }
                        break;
                    }
                }
                if (availSoQty >= SOQty)
                {
                    output.CreatePo = false;
                    output.NoCustomization = true;
                }
                else //Checking the quantity in Quantity at vendor field
                {
                    PXSelectBase<InventoryItem> vendorQty = new PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
                    And<InventoryItemExtn.usrQuantityatVendor, IsNotNull,
                    And<InventoryItemExtn.usrQuantityatVendor, Greater<Required<InventoryItemExtn.usrQuantityatVendor>>>>>>(Base);

                    decimal balQty = SOQty - availSoQty;
                    InventoryItem item = vendorQty.SelectSingle(filter.InventoryID, 0);
                    if (item != null)
                    {
                        InventoryItemExtn itemExt = PXCache<InventoryItem>.GetExtension<InventoryItemExtn>(item);
                        if (itemExt.UsrQuantityatVendor >= balQty)
                        {
                            availSoQty = availSoQty + balQty;
                            output.POQty = output.POQty + balQty;
                            output.CreatePo = true;
                        }
                    }
                    if (SOQty > availSoQty)
                    {
                        output.CreatePo = false;
                        output.NoCustomization = true;
                    }

                    if (output.CreatePo)
                    {
                        if (line.VendorID == null)
                        {
                            PXSelectBase<POVendorInventory> vendors = new PXSelectJoin<POVendorInventory,
                              InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POVendorInventory.vendorID>>>,
                              Where<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>>>(Base);

                            foreach (POVendorInventory r in vendors.Select(line.InventoryID))
                            {
                                line.VendorID = r.VendorID;
                                Base.Transactions.Update(line);
                                break;
                            }
                            if (string.IsNullOrEmpty(line.VendorID.ToString()))
                            {
                                throw new PXRowPersistingException(typeof(SOLine.vendorID).Name, (object)null, "'{0}' may not be empty, Vendor details not updated for the Inventory ID:" + line.TranDesc ?? "", new object[1]
                          {
                            (object) typeof (SOLine.vendorID).Name
                          });
                            }
                        }
                    }
                }
                output.QtyAvail = availSoQty;
            }
            catch (Exception ex)
            {
                throw new PXException(ex.Message);
            }
            return output;
        }

        protected virtual void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting BaseEvent)
        {
            var row = (SOOrder)e.Row;
            if (row != null)
            {
                if (Base.Document.Current != null && (e.Operation == PXDBOperation.Insert))
                {
                    //Checking the each and very so line available quantity and if the quantity is available in 99 location creating the Dropship PO
                    foreach (SOLine line in Base.Transactions.Select())
                    {
                        if (line.IsStockItem == true)
                        {
                            string branchcd = string.Empty;
                            Branch currentBranch = PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(Base, line.BranchID);

                            //Geting the branch CD based on the Branch id
                            branchcd = (currentBranch != null) ? Convert.ToString(currentBranch.BranchCD) : String.Empty;
                            //checking the so line branch id is sb branch or not
                            if (branchcd.Trim() == BranchName)
                            {
                                try
                                {
                                    /// Calling Check Available Quantity method
                                    AvailableQuantity availqty = CheckForAvailableQty(line.InventoryID ?? 0, line.SubItemID ?? 0, line.SiteID ?? 0, line.OrderQty ?? 0, line);
                                    if (availqty.CreatePo)
                                    {
                                        if (availqty.POQty == line.Qty)
                                        {
                                            line.OrderQty = availqty.POQty;
                                            line.POCreate = true;
                                            line.POSource = "D";
                                            Base.Transactions.Update(line);
                                        }
                                        else
                                        {
                                            //Creating new So line for dropship 
                                            SOLine newline = new SOLine();
                                            newline = Base.Transactions.Insert(newline);
                                            newline.BranchID = line.BranchID;
                                            newline.InventoryID = line.InventoryID;
                                            Base.Transactions.Update(newline);
                                            newline.SubItemID = line.SubItemID;
                                            Base.Transactions.Update(newline);
                                            newline.SiteID = line.SiteID;
                                            newline.OrderQty = availqty.POQty;
                                            newline.POCreate = true;
                                            newline.POSource = "D";
                                            Base.Transactions.Update(newline);

                                            //updating the so line quantity, by deducting the dropship quantity
                                            line.OrderQty = line.Qty - availqty.POQty;
                                            Base.Transactions.Update(line);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new PXException(ex.Message);
                                }
                            }
                        }
                    }
                }
            }

            //Base
            if (BaseEvent != null) BaseEvent(sender, e);
        }
    }

    #region Class for store available quantity
    public class AvailableQuantity
    {
        public decimal QtyAvail { get; set; }
        public bool CreatePo { get; set; }
        public bool NoCustomization { get; set; }
        public decimal POQty { get; set; }
    }
    #endregion
}