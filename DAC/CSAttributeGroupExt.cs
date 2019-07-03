using PX.Data;
using ShoebaccaProj;

namespace PX.Objects.CS
{
    public sealed class CSAttributeGroupExt : PXCacheExtension<CSAttributeGroup>
	{
		#region UsrCategory
		[PXDBString(10)]
		[PXUIField(DisplayName = "Category")]
		[ItemCategory]
		public string UsrCategory { get; set; }
		public abstract class usrCategory : PX.Data.BQL.BqlString.Field<usrCategory> { }
		#endregion
	}
}