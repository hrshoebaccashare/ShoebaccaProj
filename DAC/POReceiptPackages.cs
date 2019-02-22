using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PO;

namespace ShoebaccaProj
{
    //[Serializable]
    [System.SerializableAttribute()]
    //[PXCacheName(Messages.SOPackageDetail)]
    public class POReceiptPackages : IBqlTable
	{
		#region ReceiptNbr
			[PXDBString(15, IsKey=true)]
			[PXUIField(DisplayName = "Receipt Nbr")]
			[PXDefault(typeof(POReceipt.receiptNbr))]
			public virtual String ReceiptNbr { get; set; }
			public class receiptNbr : IBqlField { }
		#endregion

		#region PackageNbr
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Package Nbr")]
			public int? PackageNbr { get; set; }
			public class packageNbr : IBqlField { }
		#endregion

		#region TrackingNumber
			[PXDBString(30)]
			[PXUIField(DisplayName = "Tracking Number")]
			public virtual String TrackingNumber { get; set; }
			public class trackingNumber : IBqlField { }
		#endregion
	}
}
