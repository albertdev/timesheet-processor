using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Io
{
    /// <summary>
    /// This class reads a timesheet the way ManicTime sends it to the clipboard (tab-separated). Do mind that different settings are possible,
    /// so maybe a custom ManicTime timesheet profile might need to be made to match this code.
    ///
    /// In this case, the format is assumed to be the time format (hours:min:seconds) without rounding. No more than 1 week of data can be passed.
    /// On the other hand, less than 1 week is supported.
    /// </summary>
    public class ManicTimeParser
    {
        /// <summary>
        /// Only constructor to use.
        /// </summary>
        public ManicTimeParser()
        {
        }

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

                int numberOfTags = csv.Context.HeaderRecord.Count(x => x.StartsWith("Tag "));
                var orderedDays = ValidateAndParseDayEntries(csv, numberOfTags);
                var numberOfDays = orderedDays.Length;
                var hasTag2 = csv.Context.HeaderRecord.Any(x => x == "Tag 2");
                sheet.Days = orderedDays.ToList();
                // Skip over 'Tag X' columns and 'Notes' column
                int columnsToSkip = numberOfTags + 1;

                while (ReadNextLine(csv))
                {
                    // Last row is just a totals count. We can calculate this from scratch later
                    if (csv.GetField("Tag 1").Equals("Total"))
                    {
                        break;
                    }

                    var tagIds = Enumerable.Range(1, numberOfTags).Select(i => csv.GetField("Tag " + i)).ToArray();
                    // Chop off '#' symbol if placed at the start of the 'Tag 1' column
                    if (tagIds[0].StartsWith("#"))
                    {
                        tagIds[0] = tagIds[0].Substring(1);
                    }

                    var tagDetails = new TagDetails
                    {
                        TagIds = tagIds,
                        Notes = csv.GetField("Notes")
                    };

                    for (int i = 0; i < numberOfDays; i++)
                    {
                        var timeEntry = ValidateAndParseTimeEntry(csv, orderedDays[i], tagDetails, csv.GetField(i + columnsToSkip));
                        orderedDays[i].Entries.Add(timeEntry);
                        tagDetails.Entries.Add(timeEntry);
                    }
                    sheet.Tags.Add(tagDetails);
                }

                return sheet;
            }
        }

        private static bool ReadNextLine(CsvReader csv)
        {
            try
            {
                return csv.Read();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse CSV, got to line {csv.Context.Row}", e);
            }
        }

        private DayEntry[] ValidateAndParseDayEntries(CsvReader csv, int numberOfTags)
        {
            var header = csv.Context.HeaderRecord;

            if (header.Length < 3)
            {
                throw new Exception("Invalid number of columns in header. Was there any day data?");
            }

            var lastHeader = header.Length - 1;
            // Notes column should appear after tags
            int notesColumn = numberOfTags;
            int columnsToSkip = numberOfTags + 1;

            if ( ! Enumerable.Range(0, numberOfTags).All(i => header[i].StartsWith("Tag "))
                || ! "Notes".Equals(header[notesColumn])
                || ! "Total".Equals(header[lastHeader]))
            {
                throw new Exception("Header row not as expected");
            }

            // Subtract first tag columns, the 'Notes' column and last column 'Total'
            int numberOfDays = header.Length - (numberOfTags + 2);
            var result = new DayEntry[numberOfDays];

            for (int i = 0; i < numberOfDays; i++)
            {
                // Skips over first two columns
                var day = header[i + columnsToSkip];
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

        private static TimeEntry ValidateAndParseTimeEntry(CsvReader csv, DayEntry day, TagDetails tag, string timeSpent)
        {
            bool readOnly = false;
            if (timeSpent.StartsWith("#"))
            {
                readOnly = true;
                timeSpent = timeSpent.Substring(1);
            }
            // Entire line marked as readonly
            else if (csv.GetField("Tag 1").StartsWith("#"))
            {
                readOnly = true;
            }
            TimeSpan timeValue;
            try
            {
                timeValue = TimeSpan.ParseExact(timeSpent, "h\\:mm\\:ss", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                int lineNumber = csv.Context.RawRow;
                throw new FormatException($"Time entry for {day} on line {lineNumber} is not valid: value [{timeSpent}] could not be parsed", e);
            }

            return new TimeEntry(timeValue, day, tag, readOnly);
        }
    }
}