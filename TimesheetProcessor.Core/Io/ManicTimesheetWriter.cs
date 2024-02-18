using System;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Io
{
    /// <summary>
    /// Writes a timesheet in decimal hours format, with commas and full quoting
    /// </summary>
    public class ManicTimesheetWriter
    {
        private readonly ConfigurableTimesheetWriter _timesheetWriter;

        public ManicTimesheetWriter()
        {
            _timesheetWriter = new ConfigurableTimesheetWriter(Configure, ConvertTime);
        }

        public void WriteTimesheet(Timesheet sheet, TextWriter writer)
        {
            _timesheetWriter.WriteTimesheet(sheet, writer, true);
        }

        private void Configure(CsvConfiguration config)
        {
            config.Delimiter = ",";
            // Always quote, no exceptions
            config.ShouldQuote = (s, context) => true;
        }

        private string ConvertTime(TimeSpan duration)
        {
            var hours = Math.Round(duration.TotalHours, 2, MidpointRounding.ToEven);
            return hours.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}