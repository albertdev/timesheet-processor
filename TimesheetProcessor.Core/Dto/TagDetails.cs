using System;
using System.Collections.Generic;

namespace TimesheetProcessor.Core.Dto
{
    public class TagDetails : ICloneable
    {
        public TagDetails()
        {
        }

        private TagDetails(TagDetails original)
        {
            Tag1 = original.Tag1;
            Tag2 = original.Tag2;
            Notes = original.Notes;
        }

        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public string Notes { get; set; }
        public IList<TimeEntry> Entries { get; set; } = new List<TimeEntry>();

        public string TagId => Tag2 == null ? Tag1 : $"{Tag1}, {Tag2}";

        public TimeSpan TotalTimeSpent
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
        /// Clones this object. Note that the entries are not cloned, those are owned by the DayEntry object.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Clone()
        {
            return new TagDetails(this);
        }

        public override string ToString()
        {
            return TagId;
        }
    }
}