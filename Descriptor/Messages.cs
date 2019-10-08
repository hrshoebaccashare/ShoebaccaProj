using PX.Common;

namespace ShoebaccaProj
{
    [PXLocalizable]
    public class Messages
    {
        public const string FailedToParseVendorAttribute = "Failed to parse attribute {0} of vendor {1}. Value: {2}.";
        public const string MissingDropshipFeeItem = "Please setup a 'DROPSHIP FEE' Inventory Item for Dropship Fee items creation.";

        public const string DropShipDiscountPercentage = "Drop Ship Discount %";
        public const string DropShipItemHandlingFee = "Drop Ship Item Handling Fee";
        public const string DropShipOrderHandlingFee = "Drop Ship Order Handling Fee";

        public const string CalendarGoodsMoveDaysNotConfigured = "You must check at least one day in the calendar where goods are moved.";

        public const string BoxTypeNotFound = "Box type {0} not found.";
        public const string FailedToFindShipFromWarehouseAndCarrier = "Failed to identify a ship from warehouse and carrier to meet Deliver By date.";
        public const string FailedToFindCarrierAndMethod = "Failed to identify a carrier and method to meet Deliver By date.";
        public const string NoShipViaFound = "No Ship Via found for carrier {0} method {1}.";
    }
}
