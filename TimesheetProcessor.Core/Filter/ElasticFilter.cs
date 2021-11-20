using System;
using System.Linq;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    /// <summary>
    /// This filter searches the timesheet for an entry "Elastic", and if found, distributes that time evenly across all (writeable) entries
    /// in the sheet. Note that this happens on a day-by-day basis.
    /// </summary>
    public class ElasticFilter : IFilter
    {
        public Timesheet Filter(Timesheet original)
        {
            var elasticTagIndex = original.Tags.Select((details, index) => (details, index))
                .Where(x => x.details.Tag1.Equals("Elastic", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.index)
                .DefaultIfEmpty(-1)
                .First();
            // No "Elastic" tag, this is a no-op
            if (elasticTagIndex == -1)
            {
                return original;
            }
            var result = (Timesheet)original.Clone();
            result.Tags.RemoveAt(elasticTagIndex);

            foreach (var day in result.Days)
            {
                var elasticEntryIndexes = day.Entries.Select((entry, index) => (entry, index))
                    .Where(x => x.entry.Tag.Tag1.Equals("Elastic", StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.index)
                    .ToList();

               double totalElasticTicks = elasticEntryIndexes.Select(i => day.Entries[i].TimeSpent).Sum(x => x.Ticks);

                // Remove elastic entries from this day, from last to first so the indexes don't become invalid
                for (int j = elasticEntryIndexes.Count - 1; j >= 0; j--)
                {
                    int i = elasticEntryIndexes[j];
                    day.Entries.RemoveAt(i);
                }

                // No need to check any other entries if there's no Elastic tag
                if (! elasticEntryIndexes.Any() || totalElasticTicks == 0)
                {
                    continue;
                }

                double totalWriteableTicks = day.Entries.Where(x => ! x.Readonly).Sum(x => x.TimeSpent.Ticks);

                if (totalWriteableTicks == 0)
                {
                    throw new Exception("Elastic entry found but no writeable entries to distribute it to.");
                }

                foreach (var entry in day.Entries)
                {
                    long ticks = entry.TimeSpent.Ticks;
                    if (entry.Readonly || ticks == 0)
                    {
                        continue;
                    }
                    long newValue = ticks + (long)(ticks * totalElasticTicks / totalWriteableTicks);
                    entry.TimeSpent = new TimeSpan(newValue);
                }
            }

            return result;
        }
    }
}