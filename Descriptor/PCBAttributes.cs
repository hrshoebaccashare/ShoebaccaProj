using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;


namespace ShoebaccaProj
{
	public class ItemCategoryAttribute : PXStringListAttribute
	{
		private string categoryAttribute;

		public ItemCategoryAttribute()
		{
				
		}

		public override void CacheAttached(PXCache sender)
		{
			List<string> values = new List<string>();
			List<string> labels = new List<string>();

			if (string.IsNullOrWhiteSpace(categoryAttribute))
			{
				INSetup setup = PXSelect<INSetup>.Select(sender.Graph);

				if (setup != null)
				{
					INSetupExt setupExt = sender.Graph.Caches[typeof(INSetup)].GetExtension<INSetupExt>(setup);

					if (setupExt != null)
						categoryAttribute = setupExt.UsrCategoryAttribute;
				}
			}

			foreach (CSAttributeDetail detail in PXSelect<CSAttributeDetail,
																										Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>,
																										OrderBy<Asc<CSAttributeDetail.sortOrder>>>
																						.Select(sender.Graph, categoryAttribute)) //Replace with Const or Setting field?
			{
				values.Add(detail.ValueID);
				labels.Add(detail.Description);
			}

			this._AllowedValues= values.ToArray();
			this._AllowedLabels= labels.ToArray();
		}
	}
}