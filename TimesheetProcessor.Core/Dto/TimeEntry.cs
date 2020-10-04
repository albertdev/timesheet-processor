using System;

namespace TimesheetProcessor.Core.Dto
{
    public class TimeEntry
    {
        public TimeSpan TimeSpent { get; set; }
        public DayEntry Day { get; set; }
        public TagDetails Tag { get; set; }
        
        /// <summary>
        /// Indicates that this time value should not be scaled or otherwise modified.
        /// </summary>
        public bool Readonly { get; set; }
    }
}