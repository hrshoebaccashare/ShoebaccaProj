using System;
using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.SO
{
	public sealed class SOOrderShipmentExt : PXCacheExtension<PX.Objects.SO.SOOrderShipment>
	{
		#region UsrShipmentTrackingNbr
		[PXDBString(50)]
		[PXUIField(DisplayName = "Shipment Tracking Number")]
		public string UsrShipmentTrackingNbr { get; set; }
		public abstract class usrShipmentTrackingNbr : PX.Data.BQL.BqlString.Field<usrShipmentTrackingNbr> { }
		#endregion
	}
}