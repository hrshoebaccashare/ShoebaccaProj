using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.TM;
using PX.Objects.EP;
using PX.Objects.DR;
using PX.Objects.CR;
using PX.Objects.AP;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.IN;


namespace PX.Objects.IN
{
    public class InventoryItemExtn : PXCacheExtension<PX.Objects.IN.InventoryItem>
    {

        #region UsrParentSKUID



        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.IN.InventoryItem), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Parent SKU ID")]

        public virtual string UsrParentSKUID { get; set; }
        public abstract class usrParentSKUID : IBqlField { }
        #endregion

        #region UsrFRUPC



        [PXDBString(100, IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "UPC Code")]

        public virtual string UsrFRUPC { get; set; }
        public abstract class usrFRUPC : IBqlField { }
        #endregion
        
    }

}



