using System;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.PO;
using PX.TM;


namespace ShoebaccaProj
{
    public class POOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (Base.Document.Current != null && Base.Document.Cache.GetStatus(Base.Document.Current) == PXEntryStatus.Inserted && Base.Document.Current.OrderType == POOrderType.DropShip)
            {
                var row = Base.Document.Current;

                //Copy ship via from SO
                SOOrder salesOrder = SelectFrom<SOOrder>
                   .Where<SOOrder.orderType.IsEqual<@P.AsString>
                       .And<SOOrder.orderNbr.IsEqual<@P.AsString>>>
                   .View
                   .ReadOnly
                   .Select(Base, row.SOOrderType, row.SOOrderNbr);

                if(salesOrder != null)
                {
                    row.ShipVia = salesOrder.ShipVia;
                }

                //Get the Inventory Item for dropship fees to use for defaulting values
                InventoryItem dropshipFeeItem = SelectFrom<InventoryItem>
                    .Where<InventoryItem.inventoryCD.IsEqual<@P.AsString>>
                    .View
                    .ReadOnly
                    .Select(Base, "DROPSHIP FEE");

                //If item is null, just bail because you can't ad items anyway
                if (dropshipFeeItem == null)
                {
                    throw new PXException(ShoebaccaProj.Messages.MissingDropshipFeeItem);
                }

                var fees = GetVendorDropshipFees(row.VendorID.Value);
                if(fees.DiscountPercentage != 0 && !DropshipFeesItemExistOnInvoice(dropshipFeeItem.InventoryID.Value, Messages.DropShipDiscountPercentage))
                {
                    decimal totalCost = 0;
                    foreach (POLine line in Base.Transactions.Select())
                    {
                        if (line.InventoryID != dropshipFeeItem.InventoryID)
                        {
                            totalCost += line.ExtCost.Value;
                        }
                    }

                    decimal discount = Decimal.Round((fees.DiscountPercentage / 100) * totalCost, 2);
                    AddDropshipFees(dropshipFeeItem.InventoryID.Value, Messages.DropShipDiscountPercentage, 1, -discount);
                }

                if(fees.ItemHandlingFee != 0 && !DropshipFeesItemExistOnInvoice(dropshipFeeItem.InventoryID.Value, Messages.DropShipItemHandlingFee))
                {
                    int lineCount = 0;
                    foreach (POLine line in Base.Transactions.Select())
                    {
                        if (line.InventoryID != dropshipFeeItem.InventoryID)
                        {
                            lineCount++;
                        }
                    }
                    
                    AddDropshipFees(dropshipFeeItem.InventoryID.Value, Messages.DropShipItemHandlingFee, lineCount, fees.ItemHandlingFee);
                }

                if (fees.OrderHandlingFee != 0 && !DropshipFeesItemExistOnInvoice(dropshipFeeItem.InventoryID.Value, Messages.DropShipOrderHandlingFee))
                {
                    AddDropshipFees(dropshipFeeItem.InventoryID.Value, Messages.DropShipOrderHandlingFee, 1, fees.OrderHandlingFee);
                }
            }
            
            baseMethod();
        }

        private VendorDropshipFees GetVendorDropshipFees(int vendorID)
        {
            var fees = new VendorDropshipFees();
            var vendorAttributes = SelectFrom<CSAnswers>
                  .InnerJoin<Vendor>
                      .On<CSAnswers.refNoteID.IsEqual<Vendor.noteID>>
                  .Where<Vendor.bAccountID.IsEqual<@P.AsInt>>
                  .View
                  .ReadOnly
                  .Select(Base, vendorID);

            foreach(CSAnswers attr in vendorAttributes)
            {
                if (String.IsNullOrEmpty(attr.Value)) continue;

                switch(attr.AttributeID)
                {
                    case "DSDISCTPCT":
                        if (!decimal.TryParse(attr.Value, out fees.DiscountPercentage))
                        {
                            throw new PXException(ShoebaccaProj.Messages.FailedToParseVendorAttribute, attr.AttributeID, vendorID, attr.Value);
                        }
                        break;
                    case "DSITMHANDL":
                        if (!decimal.TryParse(attr.Value, out fees.ItemHandlingFee))
                        {
                            throw new PXException(ShoebaccaProj.Messages.FailedToParseVendorAttribute, attr.AttributeID, vendorID, attr.Value);
                        }
                        break;
                    case "DSORDHANDL":
                        if(!decimal.TryParse(attr.Value, out fees.OrderHandlingFee))
                        {
                            throw new PXException(ShoebaccaProj.Messages.FailedToParseVendorAttribute, attr.AttributeID, vendorID, attr.Value);
                        }
                        break;
                }
            }

            return fees;
        }

        private bool DropshipFeesItemExistOnInvoice(int inventoryID, string description)
        {
            foreach(POLine line in Base.Transactions.Select())
            {
                if(line.InventoryID == inventoryID && line.TranDesc == description)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddDropshipFees(int inventoryID, string description, decimal? quantity, decimal? unitCost)
        {
            var newLine = (POLine) Base.Transactions.Insert();
            newLine.InventoryID = inventoryID;
            newLine.TranDesc = description;
            newLine.ManualPrice = true;
            newLine.OrderQty = quantity;
            newLine.CuryUnitCost = unitCost;
            Base.Transactions.Update(newLine);
        }
    }

    public class VendorDropshipFees
    {
        public decimal OrderHandlingFee;
        public decimal ItemHandlingFee;
        public decimal DiscountPercentage;
    }
}