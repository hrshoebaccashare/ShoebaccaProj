using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace ShoebaccaProj
{
	public class PCBConst
	{
		public class int01 : PX.Data.BQL.BqlInt.Constant<int01> { public int01() : base(1) { } }
		public class int02 : PX.Data.BQL.BqlInt.Constant<int02> { public int02() : base(2) { } }
		public class int03 : PX.Data.BQL.BqlInt.Constant<int03> { public int03() : base(3) { } }
		public class int04 : PX.Data.BQL.BqlInt.Constant<int04> { public int04() : base(4) { } }
		public class int05 : PX.Data.BQL.BqlInt.Constant<int05> { public int05() : base(5) { } }
		public class int06 : PX.Data.BQL.BqlInt.Constant<int06> { public int06() : base(6) { } }
		public class int07 : PX.Data.BQL.BqlInt.Constant<int07> { public int07() : base(7) { } }
		public class int08 : PX.Data.BQL.BqlInt.Constant<int08> { public int08() : base(8) { } }
        public class int09 : PX.Data.BQL.BqlInt.Constant<int09> { public int09() : base(9) { } }
        
        public class entityTypeIN : PX.Data.BQL.BqlString.Constant<entityTypeIN> { public entityTypeIN() : base("PX.Objects.IN.InventoryItem") { } }

        public class nodropshipAttribute : PX.Data.BQL.BqlString.Constant<nodropshipAttribute> { public nodropshipAttribute() : base("NODROPSHIP") { } }
    }
}