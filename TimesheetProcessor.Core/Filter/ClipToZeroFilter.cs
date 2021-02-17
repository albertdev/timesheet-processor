using System;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    /// <summary>
    /// Takes a timesheet and makes sure that no value is below zero (such a case might occur when subtracting two timesheets to get the difference).
    /// </summary>
    public class ClipToZeroFilter : IFilter
    {
        public Timesheet Filter(Timesheet original)
        {
            var result = (Timesheet)original.Clone();

            foreach (var day in result.Days)
            {
                foreach (var entry in day.Entries)
                {
                    if (entry.TimeSpent.Ticks < 0)
                    {
                        entry.TimeSpent = TimeSpan.Zero;
                    }
                }
            }

            return result;
        }
    }
}