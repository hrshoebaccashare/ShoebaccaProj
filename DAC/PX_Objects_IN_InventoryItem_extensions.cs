using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.IN
{
    public sealed class InventoryItemExtn : PXCacheExtension<PX.Objects.IN.InventoryItem>
    {
        #region UsrParentSKUID
        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.IN.InventoryItem), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Parent SKU ID")]
        public string UsrParentSKUID { get; set; }
        public abstract class usrParentSKUID : PX.Data.BQL.BqlString.Field<usrParentSKUID> { }
        #endregion

        #region UsrFRUPC
        [PXDBString(100, IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "UPC Code")]
        public string UsrFRUPC { get; set; }
        public abstract class usrFRUPC : PX.Data.BQL.BqlString.Field<usrFRUPC> { }
        #endregion
    }
}



