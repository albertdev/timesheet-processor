using System;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    /// <summary>
    /// The final sheet will at a later point get converted to decimal hours. This filter rounds entries to the nearest 6 minutes so that we get only
    /// 1 decimal digit (i.e. blocks of time are quantized to 6 minutes or multiples thereof).
    /// </summary>
    public class RoundToNearestDeciHourFilter : IFilter
    {
        private readonly bool _allowRoundToZero;
        private const long TicksPer6Minutes = TimeSpan.TicksPerMinute * 6;
        private const long RoundMedianOf6MinutesDiff = (TicksPer6Minutes / 2) - 1;
        private bool _allowZero;

        public RoundToNearestDeciHourFilter(bool allowRoundToZero = false)
        {
            _allowRoundToZero = allowRoundToZero;
        }
        
        public Timesheet Filter(Timesheet original)
        {
            // This assumes the timesheet isn't yet rounded, otherwise we're cloning for nothing
            var result = (Timesheet)original.Clone();
            
            foreach (var day in result.Days)
            {
                foreach (var entry in day.Entries)
                {
                    long ticks = entry.TimeSpent.Ticks;
                    // Rounds up to nearest block of 6 minutes because the integer division discards the remainder
                    long roundedValue = ((ticks + RoundMedianOf6MinutesDiff) / TicksPer6Minutes) * TicksPer6Minutes;

                    if (roundedValue == 0 && ticks >= TimeSpan.TicksPerSecond && !_allowZero)
                    {
                        // Round up towards 6 minutes, otherwise tags with just a little bit of work might drop down to zero
                        roundedValue = TicksPer6Minutes;
                    }
                    entry.TimeSpent = new TimeSpan(roundedValue);
                }
            }

            return result;
        }
    }
}