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
                WriteHeader(sheet, csv, numberOfTags);
                csv.NextRecord();
                
                foreach (var tag in sheet.Tags)
                {
                    string[] tagIds = tag.TagIds;
                    for (int i = 0; i < numberOfTags; i++)
                    {
                        csv.WriteField(i < tagIds.Length ? tagIds[i] : "");
                    }
                    csv.WriteField(tag.Notes);

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
                    csv.NextRecord();
                }
                csv.WriteField("Total");
                // First tag column is filled, write an empty cell for each next tag
                for (int i = 1; i < numberOfTags; i++)
                {
                    csv.WriteField("");
                }
                csv.WriteField("");
                foreach (var day in sheet.Days)
                {
                    csv.WriteField(_timeConverter(day.TotalTimeSpent));
                }
                csv.WriteField(_timeConverter(sheet.TotalTimeSpent));

                csv.Flush();
            }
        }

        private static void WriteHeader(Timesheet sheet, CsvWriter writer, int numberOfTags)
        {
            for (int i = 0; i < numberOfTags; i++)
            {
                writer.WriteField($"Tag {i + 1}");
            }
            writer.WriteField("Notes");
            foreach (var day in sheet.Days)
            {
                writer.WriteField(day.Day.ToShortDateString());
            }
            writer.WriteField("Total");
        }
    }
}