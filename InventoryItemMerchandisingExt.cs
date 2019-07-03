using System;
using System.Collections;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using ShoebaccaProj;

namespace PX.Objects.IN
{
    public class InventoryItemMerchangisingExt : PXGraphExtension<InventoryItemMaint>
    {
        #region Views
        public PXSelect<CSAttributeDetail2,
                                        Where<CSAttributeDetail2.attributeID, Equal<Required<CSAttributeDetail2.attributeID>>>,
                                        OrderBy<Asc<CSAttributeDetail2.sortOrder>>> AttributeDetails;
        
        public PXFilter<AttributeCaptions> Captions;

        public virtual IEnumerable captions()
        {
            INSetupExt setupExt = Base.insetup.Current.GetExtension<INSetupExt>();

            AttributeCaptions row = new AttributeCaptions();

            foreach (CSAttributeDetail2 detail in this.AttributeDetails.Select(setupExt.UsrCategoryAttribute))
            {
                switch (detail.SortOrder)
                {
                    case 1:
                        row.Caption1 = detail.Description;
                        break;
                    case 2:
                        row.Caption2 = detail.Description;
                        break;
                    case 3:
                        row.Caption3 = detail.Description;
                        break;
                    case 4:
                        row.Caption4 = detail.Description;
                        break;
                    case 5:
                        row.Caption5 = detail.Description;
                        break;
                    case 6:
                        row.Caption6 = detail.Description;
                        break;
                    case 7:
                        row.Caption7 = detail.Description;
                        break;
                    case 8:
                        row.Caption8 = detail.Description;
                        break;
                    case 9:
                        row.Caption9 = detail.Description;
                        break;
                }
            }
            
            yield return row;
        }

        #region Attribute Grids

        public PXSelectJoin<CSAnswers,
                                                RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>
                                                                                                      
                                                ,And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>
                                                    >,
                                                RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                         And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                         And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int01>>>>>>,
                                                Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                     ,And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                    >,
                                                OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes1;

        protected virtual IEnumerable attributes1()
        {
            return GenerateAttributeList(1);
        }

        public PXSelectJoin<CSAnswers,
                                                RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>,
                                                                                                       
                                                                                                        And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>,
                                                RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                         And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                         And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int02>>>>>>,
                                                Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                       , And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                    >,
                                                OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes2;

        protected virtual IEnumerable attributes2()
        {
            return GenerateAttributeList(2);
        }

        public PXSelectJoin<CSAnswers,
                                                RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>,
                                                                                                       
                                                                                                        And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>,
                                                RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                         And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                         And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int03>>>>>>,
                                                Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                     , And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                    >,
                                                OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes3;

        protected virtual IEnumerable attributes3()
        {
            return GenerateAttributeList(3);
        }

        public PXSelectJoin<CSAnswers,
                                                RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>,
                                                                                                        
                                                                                                        And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>,
                                                RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                         And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                         And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int04>>>>>>,
                                                Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                      , And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                    >,
                                                OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes4;

        protected virtual IEnumerable attributes4()
        {
            return GenerateAttributeList(4);
        }

        public PXSelectJoin<CSAnswers,
                                                RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>,

                                                                                                         And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>,
                                                RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                         And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                         And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int05>>>>>>,
                                                Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                     ,And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                    >,
                                                OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes5;

        protected virtual IEnumerable attributes5()
        {
            return GenerateAttributeList(5);
        }

        public PXSelectJoin<CSAnswers,
                                                RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>,
                                                                                                        And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>,
                                                RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                         And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                         And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int06>>>>>>,
                                                Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                     ,And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                    >,
                                                OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes6;

        protected virtual IEnumerable attributes6()
        {
            return GenerateAttributeList(6);
        }

        public PXSelectJoin<CSAnswers,
                                                RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>,
                                                                                                         And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>,
                                                RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                         And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                         And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int07>>>>>>,
                                                Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                               , And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                    >,
                                                OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes7;

        protected virtual IEnumerable attributes7()
        {
            return GenerateAttributeList(7);
        }

        public PXSelectJoin<CSAnswers,
                                                RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>,
                                                                                                        And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>,
                                                RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                         And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                         And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int08>>>>>>,
                                                Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                    ,  And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                    >,
                                                OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes8;

        protected virtual IEnumerable attributes8()
        {
            return GenerateAttributeList(8);
        }
        public PXSelectJoin<CSAnswers,
                                               RightJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>,
                                                                                                       And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>,
                                               RightJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                                                                        And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                                                                        And<CSAttributeDetail2.sortOrder, Equal<PCBConst.int08>>>>>>,
                                               Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                   , And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                   >,
                                               OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes9;

        protected virtual IEnumerable attributes9()
        {
            return GenerateAttributeList(9);
        }

        #endregion Attribute Grids

        #endregion Views

        #region CacheAttached
        [PXDefault()]
        [InventoryRaw(IsKey = true, DisplayName = "Inventory ID")]
        [PXSelector(typeof(InventoryItem.inventoryCD),
                                typeof(InventoryItem.inventoryCD),
                                typeof(InventoryItem.descr),
                                typeof(InventoryItem.itemClassID),
                                typeof(InventoryItemExtn.usrFRUPC),
                                typeof(InventoryItem.itemStatus),
                                typeof(InventoryItem.itemType),
                                typeof(InventoryItem.baseUnit),
                                typeof(InventoryItem.salesUnit),
                                typeof(InventoryItem.purchaseUnit),
                                typeof(InventoryItem.basePrice)
                                )]
        [PX.Data.EP.PXFieldDescription]
        protected virtual void InventoryItem_InventoryCD_CacheAttached(PXCache cache) { }

        #endregion CacheAttached

        #region Event Handlers
        protected virtual void AttributeCaptions_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AttributeCaptions row = (AttributeCaptions)e.Row;

            if (row == null) return;

            INSetupExt setupExt = Base.insetup.Current.GetExtension<INSetupExt>();

            this.Attributes1.AllowSelect = false;
            this.Attributes2.AllowSelect = false;
            this.Attributes3.AllowSelect = false;
            this.Attributes4.AllowSelect = false;
            this.Attributes5.AllowSelect = false;
            this.Attributes6.AllowSelect = false;
            this.Attributes7.AllowSelect = false;
            this.Attributes8.AllowSelect = false;
            this.Attributes9.AllowSelect = false;

            foreach (CSAttributeDetail2 detail in this.AttributeDetails.Select(setupExt.UsrCategoryAttribute))
            {
                switch (detail.SortOrder)
                {
                    case 1:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption1>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes1.AllowSelect = true;
                        break;
                    case 2:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption2>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes2.AllowSelect = true;
                        break;
                    case 3:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption3>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes3.AllowSelect = true;
                        break;
                    case 4:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption4>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes4.AllowSelect = true;
                        break;
                    case 5:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption5>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes5.AllowSelect = true;
                        break;
                    case 6:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption6>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes6.AllowSelect = true;
                        break;
                    case 7:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption7>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes7.AllowSelect = true;
                        break;
                    case 8:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption8>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes8.AllowSelect = true;
                        break;
                    case 9:
                        PXUIFieldAttribute.SetVisible<AttributeCaptions.caption9>(Base.Caches[typeof(AttributeCaptions)], null, true);
                        this.Attributes8.AllowSelect = true;
                        break;
                }
            }
        }

        protected virtual void CSAnswers_RowDeleting(PXCache sender, PXRowDeletingEventArgs e, PXRowDeleting del)
        {
            if (del != null)
                del.Invoke(sender, e);

            CSAnswers row = (CSAnswers)e.Row;
            InventoryItem current = Base.Item.Current;

            if (row == null) return;

            if (current == null)
                e.Cancel = true;
        }

        protected virtual void CSAnswers_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting del)
        {
            if (del != null)
                del.Invoke(sender, e);

            CSAnswers row = (CSAnswers)e.Row;
            InventoryItem current = Base.Item.Current;

            if (row == null) return;

            if (current == null)
                e.Cancel = true;
        }
        #endregion Event Handler

        #region Methods
        private PXResultset<CSAnswers, CSAttributeGroup, CSAttributeDetail2> GenerateAttributeList(int attributeNum)
        {
            PXResultset<CSAnswers, CSAttributeGroup, CSAttributeDetail2> list = new PXResultset<CSAnswers, CSAttributeGroup, CSAttributeDetail2>();

            PXSelectBase<CSAttributeGroup> query = new PXSelectJoin<CSAttributeGroup,
                                                     InnerJoin<CSAttributeDetail2, On<CSAttributeGroupExt.usrCategory, Equal<CSAttributeDetail2.valueID>,
                                                     And<CSAttributeDetail2.attributeID, Equal<Required<INSetupExt.usrCategoryAttribute>>,
                                                    And<CSAttributeDetail2.sortOrder, Equal<Required<CSAttributeDetail2.sortOrder>>,
                                                     And<CSAttributeDetail2.disabled, Equal<False>>>>>>,
                                                      Where<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>
                                                     ,And<CSAttributeGroup.entityType, Equal<PCBConst.entityTypeIN>>
                                                     >,
                                                     OrderBy<Asc<CSAttributeGroup.sortOrder>>>(Base);

            InventoryItem current = Base.Item.Current;
            INSetupExt setupExt = Base.insetup.Current.GetExtension<INSetupExt>();

            if (current != null && current.InventoryID != null && setupExt != null) //&& current.InventoryID > 0
            {
                foreach (PXResult<CSAttributeGroup, CSAttributeDetail2> item in query.Select(setupExt.UsrCategoryAttribute, attributeNum, current.ItemClassID))
                {
                    CSAttributeGroup attribute = item;
                    CSAttributeDetail2 detail = item;

                    CSAnswers answerCheck = PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>,
                                             And<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>>
                                            .Select(Base, current.NoteID, attribute.AttributeID);

                    if (answerCheck == null)
                    {
                        CSAnswers newAnswer = new CSAnswers();
                        newAnswer.AttributeID = attribute.AttributeID;
                        newAnswer.Value = string.Empty;
                        newAnswer.Order = attribute.SortOrder;
                        newAnswer.IsRequired = attribute.Required;
                        newAnswer.RefNoteID = current.NoteID;
                        list.Add(new PXResult<CSAnswers, CSAttributeGroup, CSAttributeDetail2>(newAnswer, attribute, detail));
                    }
                    else
                        list.Add(new PXResult<CSAnswers, CSAttributeGroup, CSAttributeDetail2>(answerCheck, attribute, detail));
                }
            }
            return list;
        }

        #endregion Methods

    }

    #region Classes

    [Serializable]
    public class AttributeCaptions : IBqlTable
    {
        #region Caption1
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption1", Enabled = false, Visible = false)]
        public virtual string Caption1 { get; set; }
        public abstract class caption1 : IBqlField { }
        #endregion

        #region Caption2
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption2", Enabled = false, Visible = false)]
        public virtual string Caption2 { get; set; }
        public abstract class caption2 : IBqlField { }
        #endregion

        #region Caption3
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption3", Enabled = false, Visible = false)]
        public virtual string Caption3 { get; set; }
        public abstract class caption3 : IBqlField { }
        #endregion

        #region Caption4
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption4", Enabled = false, Visible = false)]
        public virtual string Caption4 { get; set; }
        public abstract class caption4 : IBqlField { }
        #endregion

        #region Caption5
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption5", Enabled = false, Visible = false)]
        public virtual string Caption5 { get; set; }
        public abstract class caption5 : IBqlField { }
        #endregion

        #region Caption6
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption6", Enabled = false, Visible = false)]
        public virtual string Caption6 { get; set; }
        public abstract class caption6 : IBqlField { }
        #endregion

        #region Caption7
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption7", Enabled = false, Visible = false)]
        public virtual string Caption7 { get; set; }
        public abstract class caption7 : IBqlField { }
        #endregion

        #region Caption8
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption8", Enabled = false, Visible = false)]
        public virtual string Caption8 { get; set; }
        public abstract class caption8 : IBqlField { }
        #endregion

        #region Caption9
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Caption9", Enabled = false, Visible = false)]
        public virtual string Caption9 { get; set; }
        public abstract class caption9 : IBqlField { }
        #endregion
    }

    [Serializable]
    [PXPossibleRowsList(typeof(CSAttribute.description), typeof(CSAnswers3.attributeID), typeof(CSAnswers3.value))]
    public class CSAnswers3 : CSAnswers
    {
        [PXDBString(10, InputMask = "", IsKey = true, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Attribute", Enabled = false)]
        [CSAttributeSelector]
        public override string AttributeID { get; set; }
        public new abstract class attributeID : IBqlField, IBqlOperand { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Value")]
        [CSAttributeValueValidation(typeof(CSAnswers.attributeID))]
        public override string Value { get; set; }
        public new abstract class value : IBqlField, IBqlOperand { }

        public new abstract class isRequired : IBqlField, IBqlOperand { }

        public new abstract class order : IBqlField, IBqlOperand { }

        public new abstract class refNoteID : IBqlField, IBqlOperand { }

    }

    [Serializable]
    public class CSAttributeDetail2 : CSAttributeDetail
    {
 
        [PXDBString(10, IsKey = true)]
        [PXUIField(DisplayName = "Attribute ID")]
        public override string AttributeID { get; set; }
        public new abstract class attributeID : IBqlField, IBqlOperand { }

        public new abstract class valueID : IBqlField, IBqlOperand { }

        public new abstract class description : IBqlField, IBqlOperand { }

        public new abstract class sortOrder : IBqlField, IBqlOperand { }

        public new abstract class disabled : IBqlField, IBqlOperand { }

        public new abstract class Tstamp : IBqlField, IBqlOperand { }

        public new abstract class createdByID : IBqlField, IBqlOperand { }

        public new abstract class createdByScreenID : IBqlField, IBqlOperand { }

        public new abstract class createdDateTime : IBqlField, IBqlOperand { }

        public new abstract class lastModifiedByID : IBqlField, IBqlOperand { }

        public new abstract class lastModifiedByScreenID : IBqlField, IBqlOperand { }

        public new abstract class lastModifiedDateTime : IBqlField, IBqlOperand { }
   
    }
    #endregion Classes
}