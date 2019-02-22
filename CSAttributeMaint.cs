//Customization
//1. A new MultiSelect Drop Combo box field for "Sales Category" is introduced in the Attributes Screen SM.000000. in details section.
//2. Users will select the "Sales Category for each value of the attribute 
//3. In Stock Items Screen, each Attribute of type "Combo Box" will be affected by this Customization  
//4. The stock item Attributes will show the values based on the "Sales Category" shown in the Attributes Tab.
//5. When a Sales Category is changed in "Stock Items/Attributes" tab, the user has to "Save" the changes in order to get the Attributes values based on the Sales Category.



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.CS
{
	
	public class CSAttributeMaint_Extension:PXGraphExtension<CSAttributeMaint>
	{

        #region Event Handlers

        //// Dynamically fill the Sales Category Dropdown list values.

        //protected virtual void CSAttributeDetail_UsrSBSalesCategories_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        //{
        //    CSAttributeDetail row = e.Row as CSAttributeDetail;
        //    if (row == null)
        //    {
        //        return;
        //    }
        //    List<string> allowedValues = new List<string>();
        //    List<string> allowedLabels = new List<string>();
        //    PXResultset<INCategory> Category = PXSelect<INCategory>.Select(Base);

        //    foreach (INCategory Items in Category)
        //    {
        //        allowedValues.Add(Items.CategoryID.ToString() ?? string.Empty);
        //        allowedLabels.Add(Items.Description);
        //    }
        //    e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 510, new bool?(true), typeof(INCategory.description).Name, new bool?(false), new int?(-1), "", allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
        //    ((PXStringState)e.ReturnState).MultiSelect = true;
        //}

        #endregion

	}


}

