using System;
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


namespace ShoebaccaProj
{
    public class POOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        public PXSelect<POLine> POline;

        protected virtual void POOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            POOrder row = e.Row as POOrder;
            if (row == null) return;
            decimal value = 0;
            decimal lineTotal = 0;
            POLine newLine;

            if (e.Operation != PXDBOperation.Delete)
            {
                if (row.OrderType == "DP")    // KN - CN/JN 2Dec16, Added condition, So that The newline is only added for DropShip type PO's
                {
                    if (row.SOOrderNbr != null && row.SOOrderType != null && row.OrderType == "DP" && e.Operation == PXDBOperation.Insert)
                    {
                        //Copy ShipVia from Sales Order to Purchase Order
                        SOOrder SOord = PXSelect<SOOrder,
                                                                        Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                                                                        And<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>>
                                                                        .Select(Base, row.SOOrderNbr, row.SOOrderType);
                        //Commented by bhavya to change the shipvia at PO level on 2018/26/10
                        //  if (SOord != null) row.ShipVia = SOord.ShipVia;
                        // revoked the commented line and added the  appended the condition check  e.Operation == PXDBOperation.Insert at the top by deebhan on 18th Jan 2019
                        if (SOord != null) row.ShipVia = SOord.ShipVia;
                    }

                    //Get the Inventory Item for dropship fees to use for defaulting values
                    InventoryItem item = PXSelect<InventoryItem,
                                                                                Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>
                                                                                .Select(Base, "DROPSHIP FEE");

                    //If item is null, just bail because you can't ad items anyway
                    if (item == null)
                    {
                        POline.Ask("Please setup a 'DROPSHIP FEE' Inventory Item for Dropship Fee items creation.", MessageButtons.OK);
                        return;
                    }

                    BAccount account = PXSelect<BAccount, Where<BAccount.type, Equal<Required<BAccount.type>>, And<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>>.Select(Base, BAccountType.VendorType, row.VendorID);

                    //Add Drop ship PO Line Items based on vendor attributes
                    foreach (PXResult<CSAnswers, CSAttribute> attr in PXSelectJoin<CSAnswers,
                                                                                                    InnerJoin<CSAttribute, On<CSAttribute.attributeID, Equal<CSAnswers.attributeID>>>,
                                                                                                    Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>,
                                                                                                    And<Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                                                                                                        Or<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                                                                                                        Or<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>>>>>
                                                                                                    .Select(Base, account.NoteID, "DSORDHANDL", "DSITMHANDL", "DSDISCTPCT"))
                    {
                        CSAnswers ans = attr;
                        CSAttribute atr = attr;

                        //See if this POLine already exists
                        POLine line = PXSelect<POLine,
                                                                        Where<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>,
                                                                        And<POLine.inventoryID, Equal<Required<POLine.inventoryID>>,
                                                                            And<POLine.tranDesc, Equal<Required<POLine.tranDesc>>>>>>
                                                                        .Select(Base, item.InventoryID, atr.Description);
                        if (line == null)
                        {
                            newLine = POline.Insert();
                            newLine.OrderNbr = row.OrderNbr;
                            newLine.OrderType = row.OrderType;
                            newLine.InventoryID = item.InventoryID;
                            newLine.TranDesc = atr.Description;
                            newLine.OrderQty = 1;
                            newLine.UOM = item.BaseUnit;
                            newLine.ExpenseAcctID = item.COGSAcctID;
                            newLine.ExpenseSubID = item.COGSSubID;
                          
                          
                            line = newLine;
                        }
                        switch (ans.AttributeID)
                        {
                            case "DSORDHANDL":
                                if (decimal.TryParse(ans.Value, out value))
                                {
                                    line.UnitCost = value;
                                    line.CuryUnitCost = value;
                                    line.LineAmt = value;
                                    line.CuryLineAmt = value;
                                }
                                break;
                            case "DSDISCTPCT":
                                if (decimal.TryParse(ans.Value, out value))
                                {

                                     lineTotal = 0m;
                                    foreach (POLine lineItem in PXSelect<POLine,
                                                                                                                Where<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>,
                                                                                                                And<POLine.inventoryID, NotEqual<Required<POLine.inventoryID>>>>>
                                                                                                                .Select(Base, item.InventoryID))
                                    {
                                        lineTotal += (lineItem.ExtCost ?? 0);
                                    }
                                    value = -(value / 100) * lineTotal;
                                    line.UnitCost = value;
                                    line.CuryUnitCost = value;
                                    line.LineAmt = value;
                                    line.CuryLineAmt = value;
                                }
                                break;
                            case "DSITMHANDL":

                                //if (decimal.TryParse(ans.Value, out value))
                                //{
                                //    decimal lineCount = PXSelect<POLine,
                                //                                                            Where<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>,
                                //                                                            And<POLine.inventoryID, NotEqual<Required<POLine.inventoryID>>>>>
                                //                                                            .Select(Base, item.InventoryID).Count;
                                //    line.OrderQty = lineCount;
                                //    line.UnitCost = value;
                                //    line.CuryUnitCost = value;
                                //    value *= lineCount;
                                //    line.LineAmt = value;
                                //    line.CuryLineAmt = value;
                                //}

                                // Commented  above logic and modified the below  one for the wrike ticket ID 277235649  by Deebhan on 28th Oct 2018 
                                if (decimal.TryParse(ans.Value, out value))
                                {
                                    decimal? lineqty = 0m;
                                    foreach (POLine lineItem in PXSelect<POLine,
                                                                                                              Where<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>,
                                                                                                              And<POLine.inventoryID, NotEqual<Required<POLine.inventoryID>>>>>
                                                                                                              .Select(Base, item.InventoryID))
                                    {
                                        lineqty += (lineItem.OrderQty ?? 0);
                                    }
                                    line.OrderQty = lineqty;
                                    line.ManualPrice = true;
                                    line.UnitCost = Convert.ToDecimal(ans.Value);
                                    line.CuryUnitCost = Convert.ToDecimal(ans.Value);
                                    value *= Convert.ToDecimal(lineqty);
                                    line.LineAmt = Convert.ToDecimal(ans.Value) * lineqty;
                                    line.CuryLineAmt = Convert.ToDecimal(ans.Value) * lineqty;
                                }
                                break;
                            default:
                                break;
                        }
                        POline.Update(line);
                    }
                }
            }
        }
    }
}