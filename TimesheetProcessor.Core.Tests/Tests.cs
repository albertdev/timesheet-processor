﻿using System;
using System.IO;
using System.Linq;
using TimesheetProcessor.Core.Dto;
using Xunit;
using TimesheetProcessor.Core.Filter;
using TimesheetProcessor.Core.Io;

namespace TimesheetProcessor.Core.Tests
{
    public class Tests
    {
        [Fact]
        public void TestParseFullWeek()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_nonpad.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(7, test.Days.Count);
            Assert.Equal(9, test.Tags.Count);
            Assert.Equal(new TimeSpan(30, 43, 20), test.TotalTimeSpent);
        }

        [Fact]
        public void TestParseFullWeekWithoutNotes()
        {
            Timesheet test;
            var parser = new ManicTimeParser();
            Assert.ThrowsAny<Exception>(() =>
            {
                using (var stream = new StreamReader("Testfiles/Timesheet_nonpad_missingnotes.tsv"))
                {
                    test = parser.ParseTimesheet(stream);
                }
            });
            parser = new ManicTimeParser(true);
            using (var stream = new StreamReader("Testfiles/Timesheet_nonpad_missingnotes.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(7, test.Days.Count);
            Assert.Equal(9, test.Tags.Count);
            Assert.Equal(new TimeSpan(30, 43, 20), test.TotalTimeSpent);
        }

        [Fact]
        public void TestParseHalfWeek()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_halfweek.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(5, test.Days.Count);
            Assert.Equal(8, test.Tags.Count);
            Assert.Equal(new TimeSpan(16, 42, 9), test.TotalTimeSpent);
        }

        [Fact]
        public void TestParseFailsIfMoreThanOneWeek()
        {
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_twoweeks.tsv"))
            {
                Assert.Throws<Exception>(() => parser.ParseTimesheet(stream));
            }
        }

        [Fact]
        public void TestExpectedTimeSpent()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-04").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            Assert.Equal(TimeSpan.FromHours(39), timesheet.ExpectedHoursSpent);
        }

        [Fact]
        public void TestExpectedTimeSpentHalfWeek()
        {
            // Wednesday, Thursday, Friday + weekend
            var timesheet = new Builder()
                .Day("2020-02-05").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            Assert.Equal(TimeSpan.FromHours(8 + 8 + 7), timesheet.ExpectedHoursSpent);
        }

        [Fact]
        public void TestReadOnlyTimeSpent()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-04").AddReadOnlyEntry("admin", "0:0:1")
                .AndOnDay("2020-02-05").AddReadOnlyEntry("admin", "0:0:1")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();
            Assert.Equal(TimeSpan.FromSeconds(2), timesheet.TotalTimeSpentWithReadonlyFlag);
        }

        [Fact]
        public void TestRoundUpToDeciHourFilter()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:7:1").AddEntry("building, prod2", "0:9:0")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            var filter = new RoundUpToDeciHourFilter();
            timesheet = filter.Filter(timesheet);

            Assert.Equal(new TimeSpan(0, 6, 0), timesheet.Days[0].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(0, 12, 0), timesheet.Days[0].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 24, 0), timesheet.Days[1].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 36, 0), timesheet.Tags.First(x => x.TagId == "admin").TotalTimeSpent);

            Assert.Equal(new TimeSpan(1, 6, 0), timesheet.TotalTimeSpent);
        }

        [Fact]
        public void TestRoundToNearestDeciHourFilter()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:7:1").AddEntry("building, prod2", "0:9:10")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            var filter = new RoundToNearestDeciHourFilter();
            timesheet = filter.Filter(timesheet);

            Assert.Equal(new TimeSpan(0, 6, 0), timesheet.Days[0].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(0, 12, 0), timesheet.Days[0].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 18, 0), timesheet.Days[1].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 36, 0), timesheet.Tags.First(x => x.TagId == "admin").TotalTimeSpent);

            Assert.Equal(new TimeSpan(1, 0, 0), timesheet.TotalTimeSpent);
        }

        [Fact]
        public void TestScalingFilter()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:7:1").AddEntry("building, prod2", "0:9:10")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            timesheet = new ScaleTimesheetFilter(2).Filter(timesheet);

            Assert.Equal(new TimeSpan(0, 2, 2), timesheet.Days[0].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(0, 13, 2), timesheet.Days[0].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 32, 22), timesheet.Days[1].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 2, 12), timesheet.Tags.First(x => x.TagId == "admin").TotalTimeSpent);

            Assert.Equal(new TimeSpan(0, 45, 34), timesheet.TotalTimeSpent);
        }

        [Fact]
        public void TestSubtractionFilter()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:6:0").AddEntry("building, prod1", "0:6:00")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:12:0").AddEntry("building, prod2", "0:12:0")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:6:0")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:6:0")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:6:0")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:6:0")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:6:0")
                .ToTimesheet();

            var timesheetOriginal = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:7:1").AddEntry("building, prod2", "0:9:10")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            timesheet = new SubtractTimesheetFilter(timesheetOriginal).Filter(timesheet);

            Assert.Equal(new TimeSpan(0, 4, 59), timesheet.Days[0].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(0, 5, 29), timesheet.Days[0].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 7, 49), timesheet.Days[1].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 34, 54), timesheet.Tags.First(x => x.TagId == "admin").TotalTimeSpent);

            Assert.Equal(new TimeSpan(0, 43, 13), timesheet.TotalTimeSpent);
        }

        [Fact]
        public void TestTabBasedSheetWriter()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30").AddEntry("work", "7:10:20")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:7:1").AddEntry("building, prod2", "0:9:10").AddEntry("work", "6:58:43")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:0:1").AddEntry("work", "7:39:33")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1").AddEntry("work", "8:10:01")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1").AddEntry("work", "6:05:17")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            string result;
            using (var writer = new StringWriter())
            {
                new TabBasedTimesheetWriter().WriteTimesheet(timesheet, writer, true);
                result = writer.ToString();
            }

            Assert.Contains("Total", result);
            // Check that total hours are properly output
            Assert.Contains("\t'36:26:41", result);
        }

        [Fact]
        public void TestManicTimeSheetWriter()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30").AddEntry("work", "7:10:20")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:7:1").AddEntry("building, prod2", "0:9:10").AddEntry("work", "6:58:43")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:0:1").AddEntry("work", "7:39:33")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1").AddEntry("work", "8:10:01")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1").AddEntry("work", "6:05:17")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            string result;
            using (var writer = new StringWriter())
            {
                new ManicTimesheetWriter().WriteTimesheet(timesheet, writer, true);
                result = writer.ToString();
            }

            Assert.Contains("Total", result);
            // Check that total hours are properly output
            Assert.Contains(",\"36.44\"", result);
        }

        [Fact]
        public void TestFullWeekFilterFirstHalf()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30").AddEntry("work", "7:10:20")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:7:1").AddEntry("building, prod2", "0:9:10").AddEntry("work", "6:58:43")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:0:1").AddEntry("work", "7:39:33")
                .ToTimesheet();

            timesheet = new FullWeekFilter().Filter(timesheet);

            Assert.Equal(new DateTime(2020, 02, 03), timesheet.Days[0].Day);
            Assert.Equal(new DateTime(2020, 02, 04), timesheet.Days[1].Day);
            Assert.Equal(new DateTime(2020, 02, 05), timesheet.Days[2].Day);
            Assert.Equal(new DateTime(2020, 02, 06), timesheet.Days[3].Day);
            Assert.Equal(new DateTime(2020, 02, 07), timesheet.Days[4].Day);
            Assert.Equal(new DateTime(2020, 02, 08), timesheet.Days[5].Day);
            Assert.Equal(new DateTime(2020, 02, 09), timesheet.Days[6].Day);
        }

        [Fact]
        public void TestFullWeekFilterLastHalf()
        {
            var timesheet = new Builder()
                .Day("2020-02-06").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30").AddEntry("work", "7:10:20")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1").AddEntry("work", "6:05:17")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            timesheet = new FullWeekFilter().Filter(timesheet);

            Assert.Equal(new DateTime(2020, 02, 03), timesheet.Days[0].Day);
            Assert.Equal(new DateTime(2020, 02, 04), timesheet.Days[1].Day);
            Assert.Equal(new DateTime(2020, 02, 05), timesheet.Days[2].Day);
            Assert.Equal(new DateTime(2020, 02, 06), timesheet.Days[3].Day);
            Assert.Equal(new DateTime(2020, 02, 07), timesheet.Days[4].Day);
            Assert.Equal(new DateTime(2020, 02, 08), timesheet.Days[5].Day);
            Assert.Equal(new DateTime(2020, 02, 09), timesheet.Days[6].Day);
        }

        [Fact]
        public void TestFullWeekFilterRandomDayMissing()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:1:1").AddEntry("building, prod1", "0:5:30").AddEntry("work", "7:10:20")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1").AddEntry("work", "8:10:01")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1").AddEntry("work", "6:05:17")
                .AndOnDay("2020-02-09").AddEntry("admin", "0:0:1")
                .ToTimesheet();

            timesheet = new FullWeekFilter().Filter(timesheet);

            Assert.Equal(new DateTime(2020, 02, 03), timesheet.Days[0].Day);
            Assert.Equal(new DateTime(2020, 02, 04), timesheet.Days[1].Day);
            Assert.Equal(new DateTime(2020, 02, 05), timesheet.Days[2].Day);
            Assert.Equal(new DateTime(2020, 02, 06), timesheet.Days[3].Day);
            Assert.Equal(new DateTime(2020, 02, 07), timesheet.Days[4].Day);
            Assert.Equal(new DateTime(2020, 02, 08), timesheet.Days[5].Day);
            Assert.Equal(new DateTime(2020, 02, 09), timesheet.Days[6].Day);
        }
    }
}
