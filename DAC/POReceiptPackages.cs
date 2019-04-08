using System;
using PX.Data;
using PX.Objects.PO;

namespace ShoebaccaProj
{
    [Serializable]
    public class POReceiptPackages : IBqlTable
	{
		#region ReceiptNbr
		[PXDBString(15, IsKey=true)]
		[PXUIField(DisplayName = "Receipt Nbr")]
		[PXDefault(typeof(POReceipt.receiptNbr))]
		public virtual string ReceiptNbr { get; set; }
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		#endregion

		#region PackageNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Package Nbr")]
		public int? PackageNbr { get; set; }
		public abstract class packageNbr : PX.Data.BQL.BqlInt.Field<packageNbr> { }
		#endregion

		#region TrackingNumber
		[PXDBString(30)]
		[PXUIField(DisplayName = "Tracking Number")]
		public virtual String TrackingNumber { get; set; }
		public abstract class trackingNumber : PX.Data.BQL.BqlString.Field<trackingNumber> { }
		#endregion
	}
}
