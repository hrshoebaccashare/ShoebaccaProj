using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace ShoebaccaProj
{
	public class PCBConst
	{
		public class int01 : Constant<Int32> { public int01() : base(1) { } }
		public class int02 : Constant<Int32> { public int02() : base(2) { } }
		public class int03 : Constant<Int32> { public int03() : base(3) { } }
		public class int04 : Constant<Int32> { public int04() : base(4) { } }
		public class int05 : Constant<Int32> { public int05() : base(5) { } }
		public class int06 : Constant<Int32> { public int06() : base(6) { } }
		public class int07 : Constant<Int32> { public int07() : base(7) { } }
		public class int08 : Constant<Int32> { public int08() : base(8) { } }
        public class int09 : Constant<Int32> { public int09() : base(8) { } }

        // Upgrade Change 5.30 to 6.10

        //public class entityTypeIN : Constant<string> { public entityTypeIN() : base("IN") { } }

        public class entityTypeIN : Constant<string> { public entityTypeIN() : base("PX.Objects.IN.InventoryItem") { } }
    }
}