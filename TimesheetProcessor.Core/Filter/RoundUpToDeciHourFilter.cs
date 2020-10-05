using System;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    /// <summary>
    /// The final sheet will at a later point get converted to decimal hours. This filter rounds up all entries so that we get only 1 decimal digit
    /// (i.e. blocks of time are quantized to 6 minutes or multiples thereof).
    /// </summary>
    public class RoundUpToDeciHourFilter : IFilter
    {
        private const long TicksPer6Minutes = TimeSpan.TicksPerMinute * 6;
        private const long RoundUpToNextMultipleDiff = TicksPer6Minutes - 1;
        
        public Timesheet Filter(Timesheet original)
        {
            // This assumes the timesheet isn't yet rounded, otherwise we're cloning for nothing
            var result = (Timesheet)original.Clone();
            
            foreach (var day in result.Days)
            {
                foreach (var entry in day.Entries)
                {
                    long ticks = entry.TimeSpent.Ticks;
                    // Rounds up to next multiple of 6 minutes because the integer division discards the remainder
                    long roundedValue = ((ticks + RoundUpToNextMultipleDiff) / TicksPer6Minutes) * TicksPer6Minutes;
                    entry.TimeSpent = new TimeSpan(roundedValue);
                }
            }

            return result;
        }
    }
}