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
            TagIds = original.TagIds;
            Notes = original.Notes;
        }

        /// <summary>
        /// ManicTime supports tagging a block of time with a hieararchy of tags.
        /// </summary>
        public string[] TagIds { get; set; }
        public string Notes { get; set; }
        public IList<TimeEntry> Entries { get; set; } = new List<TimeEntry>();

        /// <summary>
        /// Combine separate Tag Ids into easier to handle string.
        /// </summary>
        public string TagId => String.Join(", ", TagIds);
        /// <summary>
        /// First tag. This should always be a non-empty string.
        /// </summary>
        public string Tag1 => TagIds[0];

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