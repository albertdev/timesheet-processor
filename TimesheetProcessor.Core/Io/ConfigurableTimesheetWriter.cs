using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Io
{
    internal class ConfigurableTimesheetWriter
    {
        private readonly Action<CsvConfiguration> _configUpdater;
        private readonly Func<TimeSpan, string> _timeConverter;

        public ConfigurableTimesheetWriter(Action<CsvConfiguration> configUpdater, Func<TimeSpan, string> timeConverter)
        {
            _configUpdater = configUpdater;
            _timeConverter = timeConverter;
        }
        
        public void WriteTimesheet(Timesheet sheet, TextWriter writer, bool includeNotes)
        {

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // The config updater function is responsible for setting this
                //Delimiter = "\t",
                // Write header manually: the date format is something which the CSV helper library doesn't support or might only do with weird syntax
                HasHeaderRecord = false
            };

            _configUpdater(csvConfig);
            
            using (var csv = new CsvWriter(writer, csvConfig, true))
            {
                var numberOfTags = sheet.TagLevels;
                WriteHeader(sheet, csv, numberOfTags, includeNotes);
                csv.NextRecord();
                
                foreach (var tag in sheet.Tags)
                {
                    string[] tagIds = tag.TagIds;
                    for (int i = 0; i < numberOfTags; i++)
                    {
                        csv.WriteField(i < tagIds.Length ? tagIds[i] : "");
                    }
                    // This makes sure that something gets written when the tag details entries are somehow incorrect
                    foreach (var day in sheet.Days)
                    {
                        var entry = tag.Entries.FirstOrDefault(x => x.Day.Equals(day));
                        if (entry == null)
                        {
                            csv.WriteField(_timeConverter(TimeSpan.Zero));
                        }
                        else
                        {
                            csv.WriteField(_timeConverter(entry.TimeSpent));
                        }
                    }
                    csv.WriteField(_timeConverter(tag.TotalTimeSpent));
                    if (includeNotes)
                    {
                        csv.WriteField(tag.Notes);
                    }
                    csv.NextRecord();
                }
                csv.WriteField("Total");
                csv.WriteField("");
                foreach (var day in sheet.Days)
                {
                    csv.WriteField(_timeConverter(day.TotalTimeSpent));
                }
                csv.WriteField(_timeConverter(sheet.TotalTimeSpent));

                if (includeNotes)
                {
                    csv.WriteField("");
                }

                csv.Flush();
            }
        }

        private static void WriteHeader(Timesheet sheet, CsvWriter writer, int numberOfTags, bool includeNotes)
        {
            for (int i = 0; i < numberOfTags; i++)
            {
                writer.WriteField($"Tag {i + 1}");
            }
            foreach (var day in sheet.Days)
            {
                writer.WriteField(day.Day.ToShortDateString());
            }
            writer.WriteField("Total");
            if (includeNotes)
            {
                writer.WriteField("Notes");
            }
        }
    }
}