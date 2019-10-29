using PX.Data;

namespace PX.Objects.CS
{
    public sealed class CarrierPluginExt : PXCacheExtension<CarrierPlugin>
    {
        #region UsrUseForAutoRateShopping
        [PXDBBool]
        [PXUIField(DisplayName = "Use for Automated Rate Shopping")]
        public bool? UsrUseForAutoRateShopping { get; set; }
        public abstract class usrUseForAutoRateShopping : PX.Data.BQL.BqlBool.Field<usrUseForAutoRateShopping> { }
        #endregion

        #region UsrUseForPrimeOrders
        [PXDBBool]
        [PXUIField(DisplayName = "Use for Amazon Prime Orders")]
        public bool? UsrUseForPrimeOrders { get; set; }
        public abstract class usrUseForPrimeOrders : PX.Data.BQL.BqlBool.Field<usrUseForPrimeOrders> { }
        #endregion
    }
}