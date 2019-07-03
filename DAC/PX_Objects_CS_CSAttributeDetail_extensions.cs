using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CS
{
    public sealed class CSAttributeDetailExt : PXCacheExtension<PX.Objects.CS.CSAttributeDetail>
    {
        #region UsrSBSalesCategories
        [PXDBString(2048)]
        [PXUIField(DisplayName = "Sales Categories")]
        public string UsrSBSalesCategories { get; set; }
        public abstract class usrSBSalesCategories : PX.Data.BQL.BqlString.Field<usrSBSalesCategories> { }
        #endregion
    }
}
