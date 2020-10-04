using System;
using System.Collections.Generic;

namespace TimesheetProcessor.Core.Dto
{
    public class DayEntry
    {
        public DateTime Day { get; set; }
        public IList<TimeEntry> Entries { get; set; } = new List<TimeEntry>();

        public override string ToString()
        {
            return Day.ToShortDateString();
        }
    }
}