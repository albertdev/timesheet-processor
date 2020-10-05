using System;
using System.Collections.Generic;
using System.Globalization;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Tests
{
    public class Builder
    {
        private Timesheet _sheet = new Timesheet();
        private Dictionary<string, TagDetails> _tagMap = new Dictionary<string, TagDetails>();
        
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

            public EntryBuilder AddEntry(string tag, string timeSpent)
            {
                return AddEntry(tag, timeSpent, false);
            }

            public EntryBuilder AddReadOnlyEntry(string tag, string timeSpent)
            {
                return AddEntry(tag, timeSpent, true);
            }

            private EntryBuilder AddEntry(string tag, string timeSpent, bool readOnly)
            {
                TagDetails tagDetails;
                if (_builder._tagMap.ContainsKey(tag))
                {
                    tagDetails = _builder._tagMap[tag];
                }
                else
                {
                    int separatorIndex = tag.IndexOf(",", StringComparison.InvariantCulture);
                    tagDetails = new TagDetails
                    {
                        Tag1 = separatorIndex == -1 ? tag : tag.Substring(0, separatorIndex),
                        Tag2 = separatorIndex == -1 ? null : tag.Substring(separatorIndex + 1).TrimStart()
                    };
                    _builder._tagMap[tagDetails.TagId] = tagDetails;
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
                foreach (var tag in _builder._tagMap.Values)
                {
                    _builder._sheet.Tags.Add(tag);
                }
                return _builder._sheet;
            }
        }
    }
}