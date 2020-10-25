using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Io
{
    /// <summary>
    /// This class reads a timesheet the way ManicTime generally formats it. Do mind that different settings are possible.
    ///
    /// In this case, the format is assumed to be the time format (hours:min:seconds) without rounding. No more than 1 week of data can be passed,
    /// but less than 1 week is supported.
    /// </summary>
    public class ManicTimeParser
    {
        public Timesheet ParseTimesheet(TextReader input)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t"
            };
            using (var csv = new CsvReader(input, csvConfig))
            {
                Timesheet sheet = new Timesheet();
                csv.Read();
                csv.ReadHeader();

                var orderedDays = ValidateAndParseDayEntries(csv);
                var numberOfDays = orderedDays.Length;
                sheet.Days = orderedDays.ToList();

                while (csv.Read())
                {
                    // Last row is just a totals count. We can calculate this from scratch later
                    if (csv.GetField("Tag 1").Equals("Total") && String.IsNullOrWhiteSpace(csv.GetField("Tag 2")))
                    {
                        break;
                    }
                    var tagDetails = new TagDetails()
                    {
                        Tag1 = csv.GetField("Tag 1"),
                        Tag2 = csv.GetField("Tag 2"),
                        Notes = csv.GetField("Notes")
                    };
                    for (int i = 0; i < numberOfDays; i++)
                    {
                        // Skip over 'Tag 1' and 'Tag 2' column
                        var timeEntry = ValidateAndParseTimeEntry(orderedDays[i], tagDetails, csv.GetField(i + 2));
                        orderedDays[i].Entries.Add(timeEntry);
                        tagDetails.Entries.Add(timeEntry);
                    }
                    sheet.Tags.Add(tagDetails);
                }

                return sheet;
            }
        }

        private static DayEntry[] ValidateAndParseDayEntries(CsvReader csv)
        {
            var header = csv.Context.HeaderRecord;
            var lastHeader = header.Length - 1;

            if (header.Length < 4)
            {
                throw new Exception("Invalid number of columns in header");
            }

            if (! "Tag 1".Equals(header[0]) || ! "Tag 2".Equals(header[1]) || ! "Notes".Equals(header[lastHeader]) || ! "Total".Equals(header[lastHeader - 1]))
            {
                throw new Exception("Header row not as expected");
            }

            // Ignore first two columns 'Tag 1' and 'Tag 2', then ignore last two columns 'Total' and 'Notes'
            int numberOfDays = header.Length - 4;
            var result = new DayEntry[numberOfDays];

            for (int i = 0; i < numberOfDays; i++)
            {
                // Skips over first two columns
                var day = header[i + 2];
                DateTime parsedDay;
                try
                {
                    parsedDay = DateTime.Parse(day);
                }
                catch (Exception e)
                {
                    throw new FormatException($"Value [{day}] is not a date.", e);
                }
                var entry = new DayEntry {Day = parsedDay};
                result[i] = entry;
            }

            int weekNumber = result[0].Day.GetIso8601WeekOfYear();
            if (result[numberOfDays - 1].Day.GetIso8601WeekOfYear() != weekNumber)
            {
                throw new Exception("Days in timesheet are spread over more than 1 week!");
            }

            return result;
        }

        private static TimeEntry ValidateAndParseTimeEntry(DayEntry day, TagDetails tag, string timeSpent)
        {
            bool readOnly = false;
            if (timeSpent.StartsWith("#"))
            {
                readOnly = true;
                timeSpent = timeSpent.Substring(1);
            }
            TimeSpan timeValue;
            try
            {
                timeValue = TimeSpan.ParseExact(timeSpent, "h\\:mm\\:ss", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new FormatException($"Time entry for {day} in tag {tag.TagId} is not valid: value [{timeSpent}] could not be parsed", e);
            }

            return new TimeEntry(timeValue, day, tag, readOnly);
        }
    }
}