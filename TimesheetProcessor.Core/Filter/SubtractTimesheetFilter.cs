using System;
using System.Linq;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    /// <summary>
    /// Takes a timesheet and runs a subtraction operation on all entry values. This can be used to compare two timesheets.
    /// This does assume that both sheets have the same number of days and the exact same tags.
    /// </summary>
    public class SubtractTimesheetFilter : IFilter
    {
        private readonly Timesheet _subtractedSheet;

        public SubtractTimesheetFilter(Timesheet subtractedSheet)
        {
            _subtractedSheet = subtractedSheet;
        }

        public Timesheet Filter(Timesheet original)
        {
            var result = (Timesheet)original.Clone();

            foreach (var subtractedDay in _subtractedSheet.Days)
            {
                var day = result.Days.FirstOrDefault(x => x.Day == subtractedDay.Day) ?? throw new Exception($"Day {subtractedDay} not found");

                foreach (var subtractedEntry in subtractedDay.Entries)
                {
                    // Special case: Ignore the elastic time, it should get absorbed in other entries
                    if (subtractedEntry.Tag.TagId.Equals(ElasticFilter.ElasticTagName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    var entry = day.Entries.FirstOrDefault(x => x.Tag.Equals(subtractedEntry.Tag))
                                          ?? throw new Exception($"{day} is missing entry {subtractedEntry.Tag.TagId}");

                    long ticks = entry.TimeSpent.Ticks - subtractedEntry.TimeSpent.Ticks;
                    entry.TimeSpent = new TimeSpan(ticks);
                }
            }

            return result;
        }
    }
}