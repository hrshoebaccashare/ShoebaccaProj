using PX.Common.Collection;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoebaccaProj
{

    public class CRCustomAttribute
    {
        public class Attribute
        {
            public readonly string ID;
            public readonly string Description;
            public readonly int? ControlType;
            public readonly string EntryMask;
            public readonly string RegExp;
            public readonly string List;
            public readonly bool IsInternal;

            public Attribute(PXDataRecord record)
            {
                ID = record.GetString(0);
                Description = record.GetString(1);
                ControlType = record.GetInt32(2);
                EntryMask = record.GetString(3);
                RegExp = record.GetString(4);
                List = record.GetString(5);
                IsInternal = record.GetBoolean(6) == true;
                Values = new List<AttributeValue>();
            }

            protected Attribute(Attribute clone)
            {
                this.ID = clone.ID;
                this.Description = clone.Description;
                this.ControlType = clone.ControlType;
                this.EntryMask = clone.EntryMask;
                this.RegExp = clone.RegExp;
                this.List = clone.List;
                this.IsInternal = clone.IsInternal;
                this.Values = clone.Values;
            }

            public void AddValue(AttributeValue value)
            {
                Values.Add(value);
            }

            public readonly List<AttributeValue> Values;
        }

        public class AttributeValue
        {
            public readonly string ValueID;
            public readonly string Description;
            public readonly bool Disabled;
            public readonly string CategoryID;

            public AttributeValue(PXDataRecord record)
            {
                ValueID = record.GetString(1);
                Description = record.GetString(2);
                CategoryID = record.GetString(4);
                Disabled = record.GetBoolean(3) == true;
            }
        }

        public class AttributeExt : Attribute
        {
            public readonly string DefaultValue;
            public readonly bool Required;

            public AttributeExt(Attribute attr, string defaultValue, bool required)
                : base(attr)
            {
                this.DefaultValue = defaultValue;
                this.Required = required;
            }
        }

        public class AttributeList : DList<string, Attribute>
        {
            private readonly bool useDescriptionAsKey;
            public AttributeList(bool useDescriptionAsKey = false)
                : base(StringComparer.InvariantCultureIgnoreCase, DefaultCapacity, DefaultDictionaryCreationThreshold, true)
            {
                this.useDescriptionAsKey = useDescriptionAsKey;
            }
            protected override string GetKeyForItem(Attribute item)
            {
                return useDescriptionAsKey ? item.Description : item.ID;
            }

            public override Attribute this[string key]
            {
                get
                {
                    Attribute e;
                    return TryGetValue(key, out e) ? e : null;

                }
            }
        }

        public class ClassAttributeList : DList<string, AttributeExt>
        {
            public ClassAttributeList()
                : base(StringComparer.InvariantCultureIgnoreCase, DefaultCapacity, DefaultDictionaryCreationThreshold, true)
            {

            }
            protected override string GetKeyForItem(AttributeExt item)
            {
                return item.ID;
            }
            public override AttributeExt this[string key]
            {
                get
                {
                    AttributeExt e;
                    return TryGetValue(key, out e) ? e : null;
                }
            }
        }

        private class CustDefinition : IPrefetchable
        {
            public readonly AttributeList Attributes;
            public readonly AttributeList AttributesByDescr;
            public readonly Dictionary<string, AttributeList> EntityAttributes;
            public readonly Dictionary<string, Dictionary<string, ClassAttributeList>> ClassAttributes;

            public CustDefinition()
            {
                Attributes = new AttributeList();
                AttributesByDescr = new AttributeList(true);
                ClassAttributes = new Dictionary<string, Dictionary<string, ClassAttributeList>>(StringComparer.InvariantCultureIgnoreCase);
                EntityAttributes = new Dictionary<string, AttributeList>(StringComparer.InvariantCultureIgnoreCase);
            }
            public void Prefetch()
            {
                using (new PXConnectionScope())
                {
                    Attributes.Clear();
                    AttributesByDescr.Clear();
                    foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttribute>(
                        new PXDataField(typeof(CSAttribute.attributeID).Name),
                        new PXDataField(typeof(CSAttribute.description).Name),
                        new PXDataField(typeof(CSAttribute.controlType).Name),
                        new PXDataField(typeof(CSAttribute.entryMask).Name),
                        new PXDataField(typeof(CSAttribute.regExp).Name),
                        new PXDataField(typeof(CSAttribute.list).Name),
                        new PXDataField(typeof(CSAttribute.isInternal).Name)                
                        ))
                    {
                        Attribute attr = new Attribute(record);
                        Attributes.Add(attr);
                        AttributesByDescr.Add(attr);
                    }

                    foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttributeDetail>(
                        new PXDataField(typeof(CSAttributeDetail.attributeID).Name),
                        new PXDataField(typeof(CSAttributeDetail.valueID).Name),
                        new PXDataField(typeof(CSAttributeDetail.description).Name),
                        new PXDataField(typeof(CSAttributeDetail.disabled).Name),
                        new PXDataFieldOrder(typeof(CSAttributeDetail.attributeID).Name),
                        new PXDataFieldOrder(typeof(CSAttributeDetail.sortOrder).Name),
                        new PXDataField(typeof(CSAttributeDetailExt.usrSBSalesCategories).Name)
                        ))
                    {
                        string id = record.GetString(0);
                        Attribute attr;
                        if (Attributes.TryGetValue(id, out attr))
                        {
                            attr.AddValue(new AttributeValue(record));
                        }

                    }

                    // Changed the Type to EntityType. Type is depricated Field

                    foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttributeGroup>(
                       new PXDataField(typeof(CSAttributeGroup.entityType).Name),
                       new PXDataField(typeof(CSAttributeGroup.entityClassID).Name),
                       new PXDataField(typeof(CSAttributeGroup.attributeID).Name),
                       new PXDataField(typeof(CSAttributeGroup.defaultValue).Name),
                       new PXDataField(typeof(CSAttributeGroup.required).Name),
                       new PXDataFieldOrder(typeof(CSAttributeGroup.entityType).Name),
                       new PXDataFieldOrder(typeof(CSAttributeGroup.entityClassID).Name),
                       new PXDataFieldOrder(typeof(CSAttributeGroup.sortOrder).Name),
                       new PXDataFieldOrder(typeof(CSAttributeGroup.attributeID).Name)))
                    {
                        string type = record.GetString(0);
                        string classID = record.GetString(1);
                        string id = record.GetString(2);

                        Dictionary<string, ClassAttributeList> dict;
                        AttributeList list;

                        if (!EntityAttributes.TryGetValue(type, out list))
                            EntityAttributes[type] = list = new AttributeList();

                        if (!ClassAttributes.TryGetValue(type, out dict))
                            ClassAttributes[type] = dict = new Dictionary<string, ClassAttributeList>(StringComparer.InvariantCultureIgnoreCase);

                        ClassAttributeList group;
                        if (!dict.TryGetValue(classID, out group))
                            dict[classID] = group = new ClassAttributeList();

                        Attribute attr;
                        if (Attributes.TryGetValue(id, out attr))
                        {
                            list.Add(attr);
                            group.Add(new AttributeExt(attr, record.GetString(3), record.GetBoolean(4) ?? false));
                        }
                    }
                }
            }
        }

        private static CustDefinition Definitions
        {
            get
            {
                CustDefinition defs = PX.Common.PXContext.GetSlot<CustDefinition>();
                if (defs == null)
                {
                    defs = PX.Common.PXContext.SetSlot<CustDefinition>(PXDatabase.GetSlot<CustDefinition>("CRSHCustomAttribute", typeof(CSAttribute), typeof(CSAttributeDetail), typeof(CSAttributeGroup)));
                }
                return defs;
            }
        }

        public static AttributeList Attributes
        {
            get
            {
                return Definitions.Attributes;
            }
        }

        public static AttributeList AttributesByDescr
        {
            get
            {
                return Definitions.AttributesByDescr;
            }
        }

        public static AttributeList EntityAttributes(string type)
        {
            AttributeList list;
            return Definitions.EntityAttributes.TryGetValue(type, out list) ? list : new AttributeList();
        }

        public static ClassAttributeList EntityAttributes(string type, string classID)
        {
            Dictionary<string, ClassAttributeList> typeList;
            ClassAttributeList list;
            if (type != null && classID != null &&
                Definitions.ClassAttributes.TryGetValue(type, out typeList) &&
                typeList.TryGetValue(classID, out list))
                return list;
            return new ClassAttributeList();
        }
    }
}
