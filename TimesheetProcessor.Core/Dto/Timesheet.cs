using System;
using System.Collections.Generic;
using System.Linq;

namespace TimesheetProcessor.Core.Dto
{
    public class Timesheet : ICloneable
    {
        public Timesheet() {}

        /// <summary>
        /// Copy constructor
        /// </summary>
        private Timesheet(Timesheet original)
        {
            Days = original.Days.Select(x => x.Clone()).Cast<DayEntry>().ToList();
            
            // Clone above has cloned the time entries. The tag details still need to be cloned and the cross-references need to be rebuilt.
            Tags = original.Tags.Select(x => x.Clone()).Cast<TagDetails>().ToList();
            var tagMap = Tags.ToDictionary(x => x.TagId, y => y);
            foreach (var day in Days)
            {
                foreach (var entry in day.Entries)
                {
                    var newTag = tagMap[entry.Tag.TagId];
                    entry.Tag = newTag;
                    newTag.Entries.Add(entry);
                }
            }
        }

        public IList<DayEntry> Days { get; internal set; } = new List<DayEntry>();
        public IList<TagDetails> Tags { get; private set; } = new List<TagDetails>();

        /// <summary>
        /// Indicates how deeply nested tags are used in this timesheet.
        /// </summary>
        public int TagLevels => Tags.Select(x => x.TagIds.Length).Max();
        
        public TimeSpan TotalTimeSpent {
            get {
                TimeSpan result = TimeSpan.Zero;
                foreach (var tag in Tags)
                {
                    result = result.Add(tag.TotalTimeSpent);
                }
                return result;
            }
        }

        /// <summary>
        /// Sums time entries which have been marked as "readonly". Used in scaling calculations.
        /// </summary>
        public TimeSpan TotalTimeSpentWithReadonlyFlag
        {
            get
            {
                TimeSpan result = TimeSpan.Zero;
                foreach (var day in Days)
                {
                    foreach (var entry in day.Entries)
                    {
                        if (entry.Readonly)
                        {
                            result = result.Add(entry.TimeSpent);
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Calculate how many hours there should be in this collection of work days. Saturday and Sunday are always expected to be zero.
        /// </summary>
        public TimeSpan ExpectedHoursSpent => Days.Aggregate(TimeSpan.Zero, (acc, day) => acc.Add(day.ExpectedHoursSpent));

        public int WeekNumber
        {
            get
            {
                if (Days == null || Days.Count < 1)
                {
                    throw new Exception("No days loaded in sheet");
                }

                return Days[0].Day.GetIso8601WeekOfYear();
            }
        }

        public object Clone()
        {
            return new Timesheet(this);
        }
    }
}