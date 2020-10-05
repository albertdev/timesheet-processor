using System;
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
            // No-op
            // if (original.Days.Count == 7)
            // {
                return original;
            // }
            
            // var result = (Timesheet)original.Clone();

            // var firstDay = result.Days[0];
            // if (firstDay.Day.DayOfWeek == DayOfWeek.Monday)
        }
    }
}