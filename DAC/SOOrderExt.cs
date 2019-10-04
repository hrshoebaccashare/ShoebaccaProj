using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.SO;
using PX.Data;

namespace ShoebaccaProj
{
    public sealed class SOOrderExt : PXCacheExtension<SOOrder>
    {
        #region UsrOrderPlacedTimestamp
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Placed Date")]
        public DateTime? UsrOrderPlacedTimestamp { get; set; }
        public abstract class usrOrderPlacedTimestamp : PX.Data.BQL.BqlBool.Field<usrOrderPlacedTimestamp> { }
        #endregion
    }
}
