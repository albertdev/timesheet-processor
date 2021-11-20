using System;
using System.IO;
using System.Linq;
using TimesheetProcessor.Core.Dto;
using TimesheetProcessor.Core.Filter;
using TimesheetProcessor.Core.Io;
using Xunit;

namespace TimesheetProcessor.Core.Tests.UnitTests
{
    public class ParserTests
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
    }
}
