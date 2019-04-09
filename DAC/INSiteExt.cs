using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
    public sealed class INSiteExt : PXCacheExtension<INSite>
    {
        #region UsrAllowShippingAutoSelection
        [PXDBBool]
        [PXUIField(DisplayName = "Allow Shipping Auto-Selection")]
        public bool? UsrAllowShippingAutoSelection { get; set; }
        public abstract class usrAllowShippingAutoSelection : PX.Data.BQL.BqlBool.Field<usrAllowShippingAutoSelection> { }
        #endregion
    }
}