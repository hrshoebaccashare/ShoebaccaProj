using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.SO.GraphExtensions.CarrierRates;

namespace ShoebaccaProj
{
    public class SOCarrierRateShoppingExt : PXGraphExtension<SOOrderEntry.CarrierRates, SOOrderEntry>
    {
        public bool Active { get; set; }
        public int? OverrideSiteID { get; set; }

        [PXOverride]
        public IList<SOPackageEngine.PackSet> CalculatePackages(Document doc, string carrierID, Func<Document, string, IList<SOPackageEngine.PackSet>> baseMethod)
        {
            if (Active && OverrideSiteID != null)
            {
                var order = (SOOrder) Base1.Documents.Cache.GetMain(doc);
                return BuildPackageListForRateShopping(order, OverrideSiteID.Value);
            }
            else
            {
                return baseMethod(doc, carrierID);
            }
        }

        private IList<SOPackageEngine.PackSet> BuildPackageListForRateShopping(SOOrder order, int siteID)
        {
            //Shoebacca custom auto-packaging algorithm. This could be implemented as a full custom package engine (by inheriting from SOPackageEngine) 
            //but it requires us to change the configuration of every item and has other impacts.
            //Note: this assumes that the whole order is going to ship; doesn't account for backordered items.
            decimal totalQty = 0;
            decimal totalWeight = 0;
            decimal totalValue = 0;

            var ps = new SOPackageEngine.PackSet(siteID);
            foreach(SOLine line in PXSelect<SOLine>.Search<SOLine.orderType, SOLine.orderNbr, SOLine.completed>(Base, order.OrderType, order.OrderNbr, false))
            {
                if(line.IsStockItem == true)
                { 
                    totalQty += line.BaseOpenQty.GetValueOrDefault();
                    if(line.BaseOrderQty > 0) //Shouldn't happen, but just in case....
                    { 
                        totalWeight += line.ExtWeight.GetValueOrDefault() * (line.BaseOpenQty.GetValueOrDefault() / line.BaseOrderQty.GetValueOrDefault());
                        totalValue += line.ExtPrice.GetValueOrDefault() * (line.BaseOpenQty.GetValueOrDefault() / line.BaseOrderQty.GetValueOrDefault());
                    }
                }
            }

            while (totalQty > 0)
            {
                string boxID = string.Empty;
                decimal boxQty = 0;

                //TODO: This could be table-driven instead of being hardcoded...
                if (totalQty > 6)
                {
                    boxID = "10 PACK";
                    boxQty = Math.Min(totalQty, 10);
                }
                else if (totalQty > 4)
                {
                    boxID = "06 PACK";
                    boxQty = Math.Min(totalQty, 6);
                }
                else if (totalQty == 4)
                {
                    boxID = "04 PACK";
                    boxQty = totalQty;
                }
                else if (totalQty == 3)
                {
                    boxID = "03 PACK";
                    boxQty = totalQty;
                }
                else if (totalQty == 2)
                {
                    boxID = "02 PACK";
                    boxQty = totalQty;
                }
                else if (totalQty == 1)
                {
                    boxID = "01 PACK";
                    boxQty = totalQty;
                }

                var box = (CSBox)PXSelect<CSBox>.Search<CSBox.boxID>(Base, boxID);
                if (box == null) throw new PXException(Messages.BoxTypeNotFound, boxID);

                var packageInfo = new SOPackageInfoEx()
                {
                    BoxID = boxID,
                    SiteID = siteID,
                    DeclaredValue = boxQty / totalQty * totalValue, //This is just an approximation...
                    Length = box.Length,
                    Width = box.Width,
                    Height = box.Height,
                    Weight = boxQty / totalQty * totalWeight, //This is just an approximation...
                    BoxWeight = box.BoxWeight
                };
                packageInfo.GrossWeight = packageInfo.Weight + packageInfo.BoxWeight;
                ps.Packages.Add(packageInfo);

                totalQty -= boxQty;
            }

            return new List<SOPackageEngine.PackSet>() { ps };
        }
    }
}
