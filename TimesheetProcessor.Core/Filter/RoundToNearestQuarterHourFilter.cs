using System;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    /// <summary>
    /// The final sheet will at a later point get converted to decimal hours. This filter rounds entries to the nearest 15 minutes.
    /// </summary>
    public class RoundToNearestQuarterHourFilter : IFilter
    {
        private const long TicksPer15Minutes = TimeSpan.TicksPerMinute * 15;
        private const long RoundMedianOf15MinutesDiff = (TicksPer15Minutes / 2) - 1;
        private readonly bool _allowZero;

        public RoundToNearestQuarterHourFilter(bool allowRoundToZero = false)
        {
            _allowZero = allowRoundToZero;
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
                    // Rounds up to nearest block of 15 minutes because the integer division discards the remainder
                    long roundedValue = ((ticks + RoundMedianOf15MinutesDiff) / TicksPer15Minutes) * TicksPer15Minutes;

                    if (roundedValue == 0 && ticks >= TimeSpan.TicksPerSecond && !_allowZero)
                    {
                        // Round up towards 15 minutes, otherwise tags with just a little bit of work might drop down to zero
                        roundedValue = TicksPer15Minutes;
                    }
                    entry.TimeSpent = new TimeSpan(roundedValue);
                }
            }

            return result;
        }
    }
}