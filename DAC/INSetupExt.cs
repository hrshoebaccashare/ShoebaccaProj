using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
    public sealed class INSetupExt : PXCacheExtension<INSetup>
	{
		#region UsrCategoryAttribute
		[PXDBString(10)]
		[PXUIField(DisplayName = "Category Attribute")]
		[PXDefault]
		[PXSelector(typeof(Search<CSAttribute.attributeID>), DescriptionField = typeof(CSAttribute.description))]
		public string UsrCategoryAttribute { get; set; }
		public abstract class usrCategoryAttribute : PX.Data.BQL.BqlString.Field<usrCategoryAttribute> { }
		#endregion
	}
}