using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Tests
{
    public class Builder
    {
        private Timesheet _sheet = new Timesheet();
        private IDictionary<TagDetails, TagDetails> _tags = new Dictionary<TagDetails, TagDetails>();
        
        public Builder()
        {
        }

        public EntryBuilder Day(string date)
        {
            return new EntryBuilder(this, date);
        }
        
        public class EntryBuilder
        {
            private readonly Builder _builder;
            private DayEntry _dayEntry;

            public EntryBuilder(Builder builder, string dateValue)
            {
                _builder = builder;
                var date = DateTime.ParseExact(dateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                _dayEntry = new DayEntry
                {
                    Day = date
                };
                _builder._sheet.Days.Add(_dayEntry);
            }

            public EntryBuilder AddEntry(string tag, string timeSpent, string notes = "")
            {
                return AddEntry(tag, timeSpent, notes, false);
            }

            public EntryBuilder AddReadOnlyEntry(string tag, string timeSpent, string notes = "")
            {
                return AddEntry(tag, timeSpent, notes, true);
            }

            private EntryBuilder AddEntry(string tag, string timeSpent, string notes, bool readOnly)
            {
                //int separatorIndex = tag.IndexOf(",", StringComparison.InvariantCulture);
                var tagIds = tag.Split(',').Select(x => x.TrimStart()).ToArray();
                var tagDetails = new TagDetails
                {
                    TagIds = tagIds,
                    Notes = notes
                };
                if (_builder._tags.ContainsKey(tagDetails))
                {
                    // Use existing entry with similar hash
                    tagDetails = _builder._tags[tagDetails];
                }
                else
                {
                    
                    _builder._tags[tagDetails] = tagDetails;
                }
                var timeSpentParsed = TimeSpan.ParseExact(timeSpent, "h\\:m\\:s", CultureInfo.InvariantCulture);
                var entry = new TimeEntry(timeSpentParsed, _dayEntry, tagDetails, readOnly);
                _dayEntry.Entries.Add(entry);
                tagDetails.Entries.Add(entry);
                return this;
            }

            public EntryBuilder AndOnDay(string date)
            {
                return new EntryBuilder(_builder, date);
            }

            public Timesheet ToTimesheet()
            {
                foreach (var tag in _builder._tags.Values)
                {
                    _builder._sheet.Tags.Add(tag);
                }
                return _builder._sheet;
            }
        }
    }
}