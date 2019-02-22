using PX.Data;
using ShoebaccaProj;


namespace PX.Objects.CS
{
    public class CSAttributeGroupExt : PXCacheExtension<CSAttributeGroup>
	{

		#region UsrCategory
		[PXDBString(10)]
		[PXUIField(DisplayName = "Category")]
		[ItemCategory]
		public virtual string UsrCategory { get; set; }
		public abstract class usrCategory : IBqlField { }
		#endregion

	}
}