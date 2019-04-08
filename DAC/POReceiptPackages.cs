﻿using System;
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
		public abstract class receiptNbr : IBqlField { }
		#endregion

		#region PackageNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Package Nbr")]
		public int? PackageNbr { get; set; }
		public abstract class packageNbr : IBqlField { }
		#endregion

		#region TrackingNumber
		[PXDBString(30)]
		[PXUIField(DisplayName = "Tracking Number")]
		public virtual String TrackingNumber { get; set; }
		public abstract class trackingNumber : IBqlField { }
		#endregion
	}
}
