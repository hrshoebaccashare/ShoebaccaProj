using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CM;
using POReceipt = PX.Objects.PO.POReceipt;
using POReceiptLine = PX.Objects.PO.POReceiptLine;
using POReceiptEntry = PX.Objects.PO.POReceiptEntry;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.SO;


namespace PX.Objects.SO
{
	public class SOOrderShipmentExt : PXCacheExtension<PX.Objects.SO.SOOrderShipment>
	{
		#region UsrShipmentTrackingNbr
			[PXDBString(50)]
			[PXUIField(DisplayName = "Shipment Tracking Number")]

			public virtual string UsrShipmentTrackingNbr { get; set; }
			public abstract class usrShipmentTrackingNbr : IBqlField { }
		#endregion
	}
}