using System;
using System.Collections.Generic;
using System.Linq;

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

        protected bool Equals(TagDetails other)
        {
            return TagIds.SequenceEqual(other.TagIds) && string.Equals(Notes, other.Notes, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TagDetails)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int value = 397;
                for (var i = 0; i < this.TagIds.Length; i++)
                {
                    value ^= this.TagIds[i].GetHashCode();
                }
                return value ^ (Notes != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Notes) : 0);
            }
        }

        public static bool operator ==(TagDetails left, TagDetails right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TagDetails left, TagDetails right)
        {
            return !Equals(left, right);
        }
    }
}