using System;
using System.Collections;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using KN.SI.Extensibility.Maint;

namespace PX.Objects.IN
{
    public class InventoryItemDropshipValidationExt : PXGraphExtension<InventoryItemMaint>
    {
        public DropshipValidation DropshipValidation { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            DropshipValidation = new DropshipValidation(this.Base, this.Base.Answers);
        }
    }

    public class CompositeStockItemsDropshipValidationExt : PXGraphExtension<CompositeStockItemMaintExt>
    {
        public DropshipValidation DropshipValidation { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            DropshipValidation = new DropshipValidation(this.Base, this.Base.Answers);
        }
    }

    public class DropshipValidation
    {
        PXSelectBase<CSAnswers> _answers;

        public DropshipValidation(PXGraph graph, PXSelectBase<CSAnswers> answers)
        {
            _answers = answers;
            graph.RowPersisting.AddHandler<CSAnswers>(CSAnswers_RowPersisting);
            graph.RowUpdated.AddHandler<CSAnswers>(CSAnswers_RowUpdated);
        }

        private void CSAnswers_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

            var row = (CSAnswers)e.Row;
            if (row.AttributeID == "NODROPSHIP" && row.Value as string == "0")
            {
                foreach (CSAnswers attr in _answers.Select())
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

        private void CSAnswers_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
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
            foreach (CSAnswers attr in _answers.Select())
            {
                if (attr.AttributeID == "NODROPSHIP")
                {
                    if (attr.Value != stringValue)
                    {
                        attr.Value = stringValue;
                        _answers.Update(attr);
                        _answers.View.RequestRefresh();
                    }
                    break;
                }
            }
        }
    }
}
