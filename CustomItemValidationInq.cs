using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.Objects.GL;
using PX.Objects.DR;
using PX.Objects.PO;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.SM;
using PX.TM;
using PX.Objects.IN;
using PX.Objects.EP;



namespace ShoebaccaProj
{
	public class CustomItemValidationInq : PXGraph<CustomItemValidationInq>, PXImportAttribute.IPXPrepareItems
	{

		[PXImport(typeof(ValidationItems))]
		public PXSelect<ValidationItems> Items;
		protected virtual IEnumerable items()
		{ 
			List<ValidationItems> validationItems = new List<ValidationItems>();
			foreach (ValidationItems item in this.Caches["ValidationItems"].Cached)
			{
				validationItems.Add(item);
			}
			return validationItems;
		}


		#region Import Functions

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			try
			{
				if (values.Contains("UPCcode"))
				{
					ValidationItems item = new ValidationItems();
					item.UPCcode = (string)values["UPCcode"];
					item.Description = (string)values["Description"];
					item.AlreadyExists = false;

                    InventoryItem invItem = PXSelect<InventoryItem, Where<InventoryItemExtn.usrFRUPC, Equal<Required<InventoryItemExtn.usrFRUPC>>>>.Select(this, item.UPCcode);
					if (invItem != null)
					{
						item.AlreadyExists = true;
						item.ItemID = invItem.InventoryCD;
						item.ItemDescription = invItem.Descr;
					}
					Items.Insert(item);
				}
				else
				{
					throw new PXException("No UPCcode");
				}
			}
			catch (PXException ex)
			{
				throw new PXException(ex.Message);
			}
			return true;
		}

		public bool RowImporting(string viewName, object row) { return row == null; }

		public bool RowImported(string viewName, object row, object oldRow) { return oldRow == null; }

		public void PrepareItems(string viewName, IEnumerable items) { }

		#endregion Import Functions
	}

	[Serializable]
	public class ValidationItems : IBqlTable
	{
		#region UPCcode
		[PXString(100, IsKey=true)]
		[PXUIField(DisplayName = "UPC Code")]
		public virtual string UPCcode { get; set; }
		public abstract class uPCcode : PX.Data.BQL.BqlString.Field<uPCcode> { }
		#endregion

		#region Description
		[PXString(50)]
		[PXUIField(DisplayName = "UPC Description")]
		public virtual string Description { get; set; }
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion

		#region AlreadyExists
		[PXBool()]
		[PXUIField(DisplayName = "Already Exists")]
		public bool? AlreadyExists { get; set; }
		public abstract class alreadyExists : PX.Data.BQL.BqlBool.Field<alreadyExists> { }
		#endregion

		#region ItemID
		[PXString(50)]
		[PXUIField(DisplayName = "Item ID")]
		public virtual string ItemID { get; set; }
		public abstract class itemID : PX.Data.BQL.BqlString.Field<itemID> { }
		#endregion

		#region ItemDescription
		[PXString(50)]
		[PXUIField(DisplayName = "Item Description")]
		public virtual string ItemDescription { get; set; }
		public abstract class itemDescription : PX.Data.BQL.BqlString.Field<itemDescription> { }
		#endregion
	}

}