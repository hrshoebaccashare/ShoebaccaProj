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
	public class SOShipmentEntryExt : PXGraphExtension<SOShipmentEntry>
	{
		public PXSelect<SOPackageDetail,
											Where<SOPackageDetail.shipmentNbr, Equal<Required<SOPackageDetail.shipmentNbr>>>,
											OrderBy<Asc<SOPackageDetail.lineNbr>>> FirstTrackingNumber;

		public PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>> OrderShipment;

		[PXOverride]
		public void ConfirmShipment(SOOrderEntry docgraph, SOShipment shiporder, Action<SOOrderEntry, SOShipment> method)
		{
			method(docgraph, shiporder);
			SOPackageDetail TNbr = FirstTrackingNumber.Select(shiporder.ShipmentNbr);
			if (TNbr != null)
			{
				SOOrderShipment ShipNbr = OrderShipment.Select(shiporder.ShipmentNbr);
				if (ShipNbr != null)
				{
					ShipNbr.GetExtension<SOOrderShipmentExt>().UsrShipmentTrackingNbr = TNbr.TrackNumber;
					OrderShipment.Cache.Update(ShipNbr);
					Base.Save.Press();
				}
			}
		}
	}
}