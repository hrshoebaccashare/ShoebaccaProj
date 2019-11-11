using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.SO;
using PX.Data;

namespace ShoebaccaProj
{
    public sealed class SOShipmentExt : PXCacheExtension<SOShipment>
    {
        #region UsrDeliverByDate
        [PXDBDate]
        [PXUIField(DisplayName = "Deliver By")]
        [PXUIRequired(typeof(Where<usrGuaranteedDelivery, Equal<True>>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public DateTime? UsrDeliverByDate { get; set; }
        public abstract class usrDeliverByDate : PX.Data.BQL.BqlDateTime.Field<usrDeliverByDate> { }
        #endregion

        #region UsrGuaranteedDelivery
        [PXDBBool]
        [PXUIField(DisplayName = "Guaranteed")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? UsrGuaranteedDelivery { get; set; }
        public abstract class usrGuaranteedDelivery : PX.Data.BQL.BqlBool.Field<usrGuaranteedDelivery> { }
        #endregion

        #region UsrISPrimeOrder
        [PXDBBool]
        [PXUIField(DisplayName = "Is Prime Order")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? UsrISPrimeOrder { get; set; }
        public abstract class usrISPrimeOrder : IBqlField { }
        #endregion

        #region UsrPickListPrintedDate
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Pick List Printed Date", Enabled = false, IsReadOnly = true)]
        public DateTime? UsrPickListPrintedDate { get; set; }
        public abstract class usrPickListPrintedDate : PX.Data.BQL.BqlDateTime.Field<usrPickListPrintedDate> { }
        #endregion
    }
}
