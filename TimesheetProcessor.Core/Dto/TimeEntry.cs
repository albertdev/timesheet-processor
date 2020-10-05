using System;

namespace TimesheetProcessor.Core.Dto
{
    public class TimeEntry : ICloneable
    {
        public TimeEntry(TimeSpan timeSpent, DayEntry day, TagDetails tag, bool @readonly)
        {
            TimeSpent = timeSpent;
            Day = day;
            Tag = tag;
            Readonly = @readonly;
        }

        /// <summary>
        /// Copy constructor. The tag details need to be cloned externally!
        /// </summary>
        private TimeEntry(TimeEntry original) : this(original.TimeSpent, original.Day, original.Tag, original.Readonly)
        {
        }

        public TimeSpan TimeSpent { get; internal set; }
        public DayEntry Day { get; private set; }
        public TagDetails Tag { get; internal set; }

        /// <summary>
        /// Indicates that this time value should not be scaled or otherwise modified (in some cases it might get rounded though).
        /// </summary>
        public bool Readonly { get; private set; }

        public object Clone()
        {
            return new TimeEntry(this);
        }
    }
}