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

        public virtual void SOShipment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

            SOShipment row = e.Row as SOShipment;
            INSite warehouse = PXSelectorAttribute.Select<SOShipment.siteID>(sender, row) as INSite;

            if (warehouse != null)
            {
                //Apply logic and try to find the corresponding ship via for this warehouse.
                //Logic is hardcoded but simpler/cheaper to maintain at this point.
                string warehouseSiteCD = warehouse.SiteCD.TrimEnd();
                string newShipVia = null;

                if (row.ShipVia == "FREESHIP" && warehouseSiteCD == "SHOEBACCA")
                {
                    if (row.ShipmentWeight < 1)
                    {
                        newShipVia = "FIRSTCLASS";
                    }
                    else
                    {
                        newShipVia = "FEDEXSMART";
                    }
                }
                else if (row.ShipVia == "FREESHIP" && warehouseSiteCD == "CLKSVL")
                {
                    if (row.ShipmentWeight < 1)
                    {
                        newShipVia = "FIRSTCLASS"; //There's no distinct ship via for USPS and CLKSVL...
                    }
                    else
                    {
                        newShipVia = "CLKFEDEXSMART";
                    }
                }
                else if (row.ShipVia == "GROUND" && warehouseSiteCD == "SHOEBACCA")
                {
                    newShipVia = "FEDEXGROUND";
                }
                else if (row.ShipVia == "GROUND" && warehouseSiteCD == "CLKSVL")
                {
                    newShipVia = "CLKFEDEXGROUND";
                }
                else if (row.ShipVia == "2DAY" && warehouseSiteCD == "SHOEBACCA")
                {
                    newShipVia = "FEDEX2DAY";
                }
                else if (row.ShipVia == "2DAY" && warehouseSiteCD == "CLKSVL")
                {
                    newShipVia = "CLKFEDEX2DAY";
                }
                else if (row.ShipVia == "OVERNIGHT" && warehouseSiteCD == "SHOEBACCA")
                {
                    newShipVia = "FEDEXPOVERNIGHT";
                }
                else if (row.ShipVia == "OVERNIGHT" && warehouseSiteCD == "CLKSVL")
                {
                    newShipVia = "CLKFEDEXON";
                }

                if (newShipVia != null)
                {
                    PXTrace.WriteInformation("ShipVia {0} remapped to {1} in warehouse {2}.", row.ShipVia, newShipVia, warehouse.SiteCD);
                    row.ShipVia = newShipVia;
                }
                else
                {
                    PXTrace.WriteInformation("ShipVia not remapped. No entry in map for ship via {0} in warehouse {1}", row.ShipVia, warehouse.SiteCD);
                }
            }
        }
    }
}