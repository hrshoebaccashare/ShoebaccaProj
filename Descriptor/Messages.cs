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
    }
}
