using System;
using System.Globalization;
using System.Linq;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    /// <summary>
    /// Takes a timesheet and makes sure that all the days of the week are present. All entries on those added days will be zero.
    /// The week is supposed to start on Monday.
    /// </summary>
    public class FullWeekFilter : IFilter
    {
        public Timesheet Filter(Timesheet original)
        {
            if (original.Days.Count == 7)
            {
                return original;
            }
            
            var result = (Timesheet)original.Clone();

            var firstDayOfSheet = result.Days.Single(x => x.Day.Equals(result.Days.Min(y => y.Day)));
            int daysToPrepend = DaysToPrepend(firstDayOfSheet.Day.DayOfWeek);
            
            DateTime firstDayOfWeek = firstDayOfSheet.Day.AddDays(-daysToPrepend);
            DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);
            var currentDay = firstDayOfWeek;
            int insertPos = 0;
            while (currentDay <= lastDayOfWeek)
            {
                if (! result.Days.Any(x => x.Day.Equals(currentDay)))
                {
                    DayEntry newDay = new DayEntry() {Day = currentDay};
                    result.Days.Insert(insertPos, newDay);
                }
                insertPos++;
                currentDay = currentDay.AddDays(1);
            }

            return result;
        }

        private static int DaysToPrepend(DayOfWeek weekDay)
        {
            switch (weekDay)
            {
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                case DayOfWeek.Sunday:
                    return 6;
                default:
                    throw new Exception("The calendar is broken again");
            }
        }
    }
}