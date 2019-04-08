using System;
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

namespace PXDropShipPOExtPkg
{
    public class SOOrderEntryExt : PXGraphExtension<SOOrderEntry>
    {
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (Base.Document.Current != null && Base.Document.Cache.GetStatus(Base.Document.Current) == PXEntryStatus.Inserted)
            {
                Branch currentBranch = PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(Base, Base.Document.Current.BranchID);
                if (currentBranch.BranchCD.TrimEnd() == "SB") //Only for Shoebacca branch
                { 
                    foreach (SOLine line in Base.Transactions.Select())
                    {
                        if (!(line.IsStockItem == true && line.Operation == SOOperation.Issue)) continue;

                        decimal? qtyToDropship = GetQuantityToDropship(line.InventoryID, line.SubItemID, line.SiteID, line.OrderQty);
                        if (qtyToDropship > 0)
                        {
                            if (qtyToDropship == line.OrderQty)
                            {
                                line.POCreate = true;
                                line.POSource = INReplenishmentSource.DropShipToOrder;
                                Base.Transactions.Update(line);
                            }
                            else
                            {
                                //Order is partially dropshipped and fulfilled by available inventory -- create a new line for the quantity that will be dropshipped
                                SOLine newline = new SOLine();
                                newline.InventoryID = line.InventoryID;
                                newline.SiteID = line.SiteID;
                                newline.OrderQty = qtyToDropship; //Check if necessary
                                newline.POCreate = true;
                                newline.POSource = INReplenishmentSource.DropShipToOrder;
                                newline.SubItemID = line.SubItemID;
                                newline = Base.Transactions.Insert(newline);

                                //Update original line, deducting the quantity dropshipped
                                line.OrderQty = line.OrderQty - qtyToDropship;
                                Base.Transactions.Update(line);
                            }
                        }

                    }
                }
            }
        
            baseMethod();
        }

        private decimal? GetQuantityToDropship(int? inventoryID, int? subItemID, int? siteID, decimal? salesOrderQty)
        {
            INSiteStatus siteStatus = SelectFrom<INSiteStatus>
                .Where<INSiteStatus.inventoryID.IsEqual<@P.AsInt>
                    .And<INSiteStatus.subItemID.IsEqual<@P.AsInt>>
                    .And<INSiteStatus.siteID.IsEqual<@P.AsInt>>>
                .View
                .ReadOnly
                .Select(Base, inventoryID, subItemID, siteID);

            decimal? qtyHardAvail = siteStatus?.QtyHardAvail;
            if (qtyHardAvail <= 0) qtyHardAvail = 0;

            if (qtyHardAvail >= salesOrderQty)
            {
                return 0;
            }
            else //Checking the quantity in Quantity at vendor field
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

                decimal? qtyUnavailable = salesOrderQty - qtyHardAvail;
                if (dropshipAllowed && itemExt.UsrQuantityatVendor >= qtyUnavailable)
                {
                    //Vendor can drop-ship all the remaining quantity needed to fufill this line
                    return qtyUnavailable;
                }
                else
                {
                    //Vendor does not do drop-ships or has insufficient quantity to fulfill the remaining quantity on the line
                    return 0;
                }
            }
        }
    }
}