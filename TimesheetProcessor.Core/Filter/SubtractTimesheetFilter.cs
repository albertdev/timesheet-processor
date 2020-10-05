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
            
            foreach (var day in result.Days)
            {
                var subtractedDay = _subtractedSheet.Days.FirstOrDefault(x => x.Day == day.Day) ?? throw new Exception($"Day {day} not found");
                foreach (var entry in day.Entries)
                {
                    var subtractedEntry = subtractedDay.Entries.FirstOrDefault(x => x.Tag.TagId == entry.Tag.TagId)
                                          ?? throw new Exception($"{day} is missing entry {entry.Tag.TagId}");
                    
                    long ticks = entry.TimeSpent.Ticks - subtractedEntry.TimeSpent.Ticks;
                    entry.TimeSpent = new TimeSpan(ticks);
                }
            }

            return result;
        }
    }
}