using System;
using System.Globalization;
using System.IO;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Io
{
    public class TabBasedTimesheetWriter
    {
        private readonly ConfigurableTimesheetWriter _timesheetWriter;

        public TabBasedTimesheetWriter()
        {
            _timesheetWriter = new ConfigurableTimesheetWriter(config => config.Delimiter = "\t", ConvertTime);
        }

        public void WriteTimesheet(Timesheet sheet, TextWriter writer, bool includeNotes)
        {
            _timesheetWriter.WriteTimesheet(sheet, writer, includeNotes);
        }

        private string ConvertTime(TimeSpan duration)
        {
            // Manually build formatted string, TimeSpan format strings roll over to days if more than 24 hours have passed
            if (duration.Ticks > TimeSpan.TicksPerDay)
            {
                // Truncate remainder, as those will be pulled from dedicated properties
                int hours = (int) duration.TotalHours;

                // Include a single quote, otherwise tools like Excel will still mangle it
                return $"'{hours}:{duration.Minutes}:{duration.Seconds}";
            }

            // Rare negative duration. Can happen if rounding and other calculations somehow end up with a smaller target number. Add single quote
            if (duration.Ticks < 0)
            {
                return duration.Duration().ToString("\\'\\-h\\:mm\\:ss");
            }
            return duration.ToString("h\\:mm\\:ss", CultureInfo.InvariantCulture);
        }
    }
}