using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
    public sealed class INSetupExt : PXCacheExtension<INSetup>
	{
		#region UsrImageURL
		[PXDBString(100)]
		[PXUIField(DisplayName = "Image URL")]
		public string UsrImageURL { get; set; }
		public abstract class usrImageURL : IBqlField { }
		#endregion

		#region UsrCategoryAttribute
		[PXDBString(10)]
		[PXUIField(DisplayName = "Category Attribute")]
		[PXDefault]
		[PXSelector(typeof(Search<CSAttribute.attributeID>), DescriptionField = typeof(CSAttribute.description))]
		public string UsrCategoryAttribute { get; set; }
		public abstract class usrCategoryAttribute : IBqlField { }
		#endregion
	}
}