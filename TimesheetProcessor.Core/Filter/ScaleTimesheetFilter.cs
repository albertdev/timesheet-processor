using System;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    /// <summary>
    /// This filter will increase the timesheet hours by scaling all entries which didn't opt out by a certain factor. This way a timesheet which
    /// falls just short of the expected hours can be brought closer to the ideal.
    /// </summary>
    public class ScaleTimesheetFilter : IFilter
    {
        private const long TicksPer6Minutes = TimeSpan.TicksPerMinute * 6;
        private const long RoundMedianOf6MinutesDiff = (TicksPer6Minutes / 2) - 1;
        private readonly double _scaleFactor;

        public ScaleTimesheetFilter(double scaleFactor)
        {
            _scaleFactor = scaleFactor;
        }
        
        public Timesheet Filter(Timesheet original)
        {
            var result = (Timesheet)original.Clone();
            
            foreach (var day in result.Days)
            {
                foreach (var entry in day.Entries)
                {
                    // Some entries should not be scaled; mainly fixed time meetings or holidays
                    if (entry.Readonly)
                    {
                        continue;
                    }
                    
                    long ticks = entry.TimeSpent.Ticks;
                    // Truncate value - the ticks value is in hundreds of nanoseconds so any loss of precision goes unnoticed
                    long newValue = (long) (ticks * _scaleFactor);
                    entry.TimeSpent = new TimeSpan(newValue);
                }
            }

            return result;
        }
    }
}