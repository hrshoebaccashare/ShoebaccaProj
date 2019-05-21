using System;
using System.Collections;
using System.Text;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
    public class InventoryItemDropshipValidationExt : PXGraphExtension<InventoryItemMaint>
    {
        public void CSAnswers_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

            var row = (CSAnswers)e.Row;
            if (row.AttributeID == "NODROPSHIP" && row.Value as string == "0")
            {
                foreach (CSAnswers attr in Base.Answers.Select())
                {
                    if (attr.AttributeID == "PRODUCTBUY")
                    {
                        if (!String.Equals(attr.Value, "Dropship"))
                        {
                            sender.RaiseExceptionHandling<CSAnswers.value>(row, row.Value, new PXSetPropertyException("Dropshipping is only allowed only on items with 'Dropship' Product Buy Type."));
                        }
                        break;
                    }
                }
            }
        }
        
        public void CSAnswers_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (CSAnswers)e.Row;
            if (row.AttributeID == "PRODUCTBUY")
            {
                var isDropshipBuyType = String.Equals(row.Value, "Dropship", StringComparison.InvariantCultureIgnoreCase);
                SetNoDropshipAttribute(!isDropshipBuyType);
            }
        }
        
        private void SetNoDropshipAttribute(bool value)
        {
            string stringValue = value ? "1" : "0";
            foreach (CSAnswers attr in Base.Answers.Select())
            {
                if (attr.AttributeID == "NODROPSHIP")
                {
                    if(attr.Value != stringValue)
                    {
                        attr.Value = stringValue;
                        Base.Answers.Update(attr);
                        Base.Answers.View.RequestRefresh();
                    }
                    break;
                }
            }
        }
    }
}
