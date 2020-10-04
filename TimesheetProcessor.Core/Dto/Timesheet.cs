using System.Collections.Generic;

namespace TimesheetProcessor.Core.Dto
{
    public class Timesheet
    {
        public IList<DayEntry> Days { get; set; } = new List<DayEntry>();
        public IList<TagDetails> Tags { get; set; } = new List<TagDetails>();
    }
}