using System;
using PX.Data;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.CS;


namespace PX.Objects.CS{
public class CSAttributeDetailExt: PXCacheExtension<PX.Objects.CS.CSAttributeDetail>{


			
			#region UsrSBSalesCategories




            [PXDBString(2048)]
            [PXUIField(DisplayName="Sales Categories")]

			public virtual string UsrSBSalesCategories{get;set;}
			public abstract class usrSBSalesCategories : IBqlField{}

			#endregion




}




}
