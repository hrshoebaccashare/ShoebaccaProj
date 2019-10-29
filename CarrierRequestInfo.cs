using System;
using System.Collections.Generic;
using PX.CarrierService;
using PX.Objects.CS;

namespace ShoebaccaProj
{
    internal class CarrierRequestInfo
    {
        public CarrierPlugin Plugin;
        public int SiteID;
        public ICarrierService Service;
        public CarrierRequest Request;
        public CarrierResult<IList<RateQuote>> Result;
    }
}
