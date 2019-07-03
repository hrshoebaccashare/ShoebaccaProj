using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.PO;
using PX.TM;


namespace ShoebaccaProj
{
	public class PPOReceiptEntryExt : PXGraphExtension<POReceiptEntry>
	{       
		public PXSelect<POReceiptPackages, Where<POReceiptPackages.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>> Packages;       
	}
}