using System;
using System.Collections.Generic;
using System.Linq;

namespace TimesheetProcessor.Core.Dto
{
    public class DayEntry : ICloneable
    {
        public DayEntry()
        {
        }

        private DayEntry(DayEntry original)
        {
            Day = original.Day;
            Entries = original.Entries.Select(x => x.Clone()).Cast<TimeEntry>().ToList();
        }
        
        public DateTime Day { get; set; }
        public IList<TimeEntry> Entries { get; set; } = new List<TimeEntry>();

        public override string ToString()
        {
            return Day.ToShortDateString();
        }

        public TimeSpan TimeSpent
        {
            get
            {
                TimeSpan result = TimeSpan.Zero;
                foreach (var entry in Entries)
                {
                    result = result.Add(entry.TimeSpent);
                }

                return result;
            }
        }

        /// <summary>
        /// A full work week is expected to contain 39 hours. Friday is just 7 hours, Saturday and Sunday are expected to be free.
        /// </summary>
        public TimeSpan ExpectedHoursSpent
        {
            get
            {
                switch (Day.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        return TimeSpan.Zero;
                    case DayOfWeek.Friday:
                        return TimeSpan.FromHours(7);
                    default:
                        return TimeSpan.FromHours(8);
                }
            }
        }

        public Object Clone()
        {
            return new DayEntry(this);
        }
    }
}