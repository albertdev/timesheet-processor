using System;
using System.IO;
using TimesheetProcessor.Core.Dto;
using Xunit;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Tests
{
    public class Tests
    {
        [Fact]
        public void TestNonPadded()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_nonpad.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(7, test.Days.Count);
            Assert.Equal(9, test.Tags.Count);
            Assert.Equal(110600, test.TotalTimeSpent.TotalSeconds);
        }
        
        [Fact]
        public void TestHalfWeekOK()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_halfweek.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(5, test.Days.Count);
            Assert.Equal(8, test.Tags.Count);
            Assert.Equal(60129, test.TotalTimeSpent.TotalSeconds);
        }
        
        [Fact]
        public void TestFailsIfMoreThanOneWeek()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_twoweeks.tsv"))
            {
                Assert.Throws<Exception>(() => parser.ParseTimesheet(stream));
            }
        }
    }
}