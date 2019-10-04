using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.CS;
using PX.Data;

namespace ShoebaccaProj
{
    public sealed class CSCalendarExt : PXCacheExtension<CSCalendar>
    {
        #region UsrSunCutoffTime
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXDefault(TypeCode.DateTime, "01/01/2008 18:00:00")]
        [PXFormula(typeof(Switch<Case<Where<CSCalendar.sunGoodsMoves, Equal<False>>, Null>, DefaultValue<CSCalendarExt.usrSunCutoffTime>>))]
        [PXUIField(DisplayName = "Sunday Cutoff Time")]
        [PXUIEnabled(typeof(Where<CSCalendar.sunGoodsMoves, Equal<True>>))]
        [PXUIRequired(typeof(Where<CSCalendar.sunGoodsMoves, Equal<True>>))]
        public DateTime? UsrSunCutoffTime { get; set; }
        public abstract class usrSunCutoffTime : PX.Data.BQL.BqlBool.Field<usrSunCutoffTime> { }
        #endregion

        #region UsrMonCutoffTime
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXDefault(TypeCode.DateTime, "01/01/2008 18:00:00")]
        [PXFormula(typeof(Switch<Case<Where<CSCalendar.monGoodsMoves, Equal<False>>, Null>, DefaultValue<CSCalendarExt.usrMonCutoffTime>>))]
        [PXUIField(DisplayName = "Monday Cutoff Time")]
        [PXUIEnabled(typeof(Where<CSCalendar.monGoodsMoves, Equal<True>>))]
        [PXUIRequired(typeof(Where<CSCalendar.monGoodsMoves, Equal<True>>))]
        public DateTime? UsrMonCutoffTime { get; set; }
        public abstract class usrMonCutoffTime : PX.Data.BQL.BqlBool.Field<usrMonCutoffTime> { }
        #endregion

        #region UsrTueCutoffTime
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXDefault(TypeCode.DateTime, "01/01/2008 18:00:00")]
        [PXFormula(typeof(Switch<Case<Where<CSCalendar.tueGoodsMoves, Equal<False>>, Null>, DefaultValue<CSCalendarExt.usrTueCutoffTime>>))]
        [PXUIField(DisplayName = "Tuesday Cutoff Time")]
        [PXUIEnabled(typeof(Where<CSCalendar.tueGoodsMoves, Equal<True>>))]
        [PXUIRequired(typeof(Where<CSCalendar.tueGoodsMoves, Equal<True>>))]
        public DateTime? UsrTueCutoffTime { get; set; }
        public abstract class usrTueCutoffTime : PX.Data.BQL.BqlBool.Field<usrTueCutoffTime> { }
        #endregion

        #region UsrWedCutoffTime
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXDefault(TypeCode.DateTime, "01/01/2008 18:00:00")]
        [PXFormula(typeof(Switch<Case<Where<CSCalendar.wedGoodsMoves, Equal<False>>, Null>, DefaultValue<CSCalendarExt.usrWedCutoffTime>>))]
        [PXUIField(DisplayName = "Wednesday Cutoff Time")]
        [PXUIEnabled(typeof(Where<CSCalendar.wedGoodsMoves, Equal<True>>))]
        [PXUIRequired(typeof(Where<CSCalendar.wedGoodsMoves, Equal<True>>))]
        public DateTime? UsrWedCutoffTime { get; set; }
        public abstract class usrWedCutoffTime : PX.Data.BQL.BqlBool.Field<usrWedCutoffTime> { }
        #endregion

        #region UsrThuCutoffTime
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXDefault(TypeCode.DateTime, "01/01/2008 18:00:00")]
        [PXFormula(typeof(Switch<Case<Where<CSCalendar.thuGoodsMoves, Equal<False>>, Null>, DefaultValue<CSCalendarExt.usrThuCutoffTime>>))]
        [PXUIField(DisplayName = "Thursday Cutoff Time")]
        [PXUIEnabled(typeof(Where<CSCalendar.thuGoodsMoves, Equal<True>>))]
        [PXUIRequired(typeof(Where<CSCalendar.thuGoodsMoves, Equal<True>>))]
        public DateTime? UsrThuCutoffTime { get; set; }
        public abstract class usrThuCutoffTime : PX.Data.BQL.BqlBool.Field<usrThuCutoffTime> { }
        #endregion

        #region UsrFriCutoffTime
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXDefault(TypeCode.DateTime, "01/01/2008 18:00:00")]
        [PXFormula(typeof(Switch<Case<Where<CSCalendar.friGoodsMoves, Equal<False>>, Null>, DefaultValue<CSCalendarExt.usrFriCutoffTime>>))]
        [PXUIField(DisplayName = "Friday Cutoff Time")]
        [PXUIEnabled(typeof(Where<CSCalendar.friGoodsMoves, Equal<True>>))]
        [PXUIRequired(typeof(Where<CSCalendar.friGoodsMoves, Equal<True>>))]
        public DateTime? UsrFriCutoffTime { get; set; }
        public abstract class usrFriCutoffTime : PX.Data.BQL.BqlBool.Field<usrFriCutoffTime> { }
        #endregion

        #region UsrSatCutoffTime
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXDefault(TypeCode.DateTime, "01/01/2008 18:00:00")]
        [PXFormula(typeof(Switch<Case<Where<CSCalendar.satGoodsMoves, Equal<False>>, Null>, DefaultValue<CSCalendarExt.usrSatCutoffTime>>))]
        [PXUIField(DisplayName = "Saturday Cutoff Time")]
        [PXUIEnabled(typeof(Where<CSCalendar.satGoodsMoves, Equal<True>>))]
        [PXUIRequired(typeof(Where<CSCalendar.satGoodsMoves, Equal<True>>))]
        public DateTime? UsrSatCutoffTime { get; set; }
        public abstract class usrSatCutoffTime : PX.Data.BQL.BqlBool.Field<usrSatCutoffTime> { }
        #endregion
    }
}
