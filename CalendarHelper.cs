using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.CR;
using PX.Objects.CS;

namespace ShoebaccaProj
{
    internal class CalendarHelper
    {
        public static DateTime GetClosestWarehouseDay(PXGraph graph, string calendarID, DateTime orderDateTime)
        {
            CSCalendar calendar = PXSelect<CSCalendar>.Search<CSCalendar.calendarID>(graph, calendarID);
            CSCalendarExt calendarExt = calendar.GetExtension<CSCalendarExt>(); 

            if (calendar == null)
                throw new InvalidOperationException(PX.Objects.CR.Messages.FailedToSelectCalenderId);
            else if(calendar.SunGoodsMoves == false && calendar.MonGoodsMoves == false && calendar.TueGoodsMoves == false && calendar.WedGoodsMoves == false && calendar.ThuGoodsMoves == false && calendar.FriGoodsMoves == false && calendar.SatGoodsMoves == false && calendar.SunGoodsMoves == false)
                throw new InvalidOperationException(Messages.CalendarGoodsMoveDaysNotConfigured);

            //Identify cutoff time for order processing (we look at the time order was _placed_ to determine if it's before or after cutoff)
            TimeSpan? cutoffTime = null;
            switch (orderDateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    cutoffTime = calendarExt.UsrSunCutoffTime?.TimeOfDay;
                    break;
                case DayOfWeek.Monday:
                    cutoffTime = calendarExt.UsrMonCutoffTime?.TimeOfDay;
                    break;
                case DayOfWeek.Tuesday:
                    cutoffTime = calendarExt.UsrTueCutoffTime?.TimeOfDay;
                    break;
                case DayOfWeek.Wednesday:
                    cutoffTime = calendarExt.UsrWedCutoffTime?.TimeOfDay;
                    break;
                case DayOfWeek.Thursday:
                    cutoffTime = calendarExt.UsrThuCutoffTime?.TimeOfDay;
                    break;
                case DayOfWeek.Friday:
                    cutoffTime = calendarExt.UsrFriCutoffTime?.TimeOfDay;
                    break;
                case DayOfWeek.Saturday:
                    cutoffTime = calendarExt.UsrSatCutoffTime?.TimeOfDay;
                    break;
            }

            DateTime closestWarehouseDay;
            if(cutoffTime != null && orderDateTime.TimeOfDay > cutoffTime)
            {
                closestWarehouseDay = orderDateTime.AddDays(1).Date;
            }
            else
            {
                closestWarehouseDay = orderDateTime.Date;
            }

            //Load upcoming exceptions for next 30 days
            PXResultset<CSCalendarExceptions> calendarExceptions = PXSelect<CSCalendarExceptions, 
                Where<CSCalendarExceptions.date, Between<Required<CSCalendarExceptions.date>, Required<CSCalendarExceptions.date>>>>.Select(graph, closestWarehouseDay, closestWarehouseDay.AddMonths(1));
            
            //Based on cutoff time and exceptions, find how soon this order will ship
            bool isWarehouseDay = false;
            while(!isWarehouseDay)
            {
                CSCalendarExceptions cse = calendarExceptions.RowCast<CSCalendarExceptions>().Where(ce => ce.Date == closestWarehouseDay).SingleOrDefault();
                if (cse != null)
                {
                    isWarehouseDay = (cse.GoodsMoved == true);
                }
                else
                {
                    switch (closestWarehouseDay.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            isWarehouseDay =(calendar.SunGoodsMoves == true);
                            break;
                        case DayOfWeek.Monday:
                            isWarehouseDay =(calendar.MonGoodsMoves == true);
                            break;
                        case DayOfWeek.Tuesday:
                            isWarehouseDay = (calendar.TueGoodsMoves == true);
                            break;
                        case DayOfWeek.Wednesday:
                            isWarehouseDay = (calendar.WedGoodsMoves == true);
                            break;
                        case DayOfWeek.Thursday:
                            isWarehouseDay = (calendar.ThuGoodsMoves == true);
                            break;
                        case DayOfWeek.Friday:
                            isWarehouseDay = (calendar.FriGoodsMoves == true);
                            break;
                        case DayOfWeek.Saturday:
                            isWarehouseDay = (calendar.SatGoodsMoves == true);
                            break;
                    }
                }

                if(!isWarehouseDay)
                {
                    closestWarehouseDay = closestWarehouseDay.AddDays(1);
                }
            }

            return closestWarehouseDay;
        }
    }
}
