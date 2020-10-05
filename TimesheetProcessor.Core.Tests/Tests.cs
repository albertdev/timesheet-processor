using System;
using System.IO;
using TimesheetProcessor.Core.Dto;
using Xunit;
using TimesheetProcessor.Core.Dto;
using TimesheetProcessor.Core.Filter;

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
            Assert.Equal(new TimeSpan(30, 43, 20), test.TotalTimeSpent);
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
            Assert.Equal(new TimeSpan(16, 42, 9), test.TotalTimeSpent);
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
        
        [Fact]
        public void TestExpectedTimeSpent()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_nonpad.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(TimeSpan.FromHours(39), test.ExpectedHoursSpent);
        }
        
        [Fact]
        public void TestExpectedTimeSpentHalfWeek()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_halfweek.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(TimeSpan.FromHours(8 + 8 + 7), test.ExpectedHoursSpent);
        }
        
        [Fact]
        public void TestReadOnlyTimeSpent()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_nonpad.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(TimeSpan.FromHours(16), test.TotalTimeSpentWithReadonlyFlag);
        }
        
        [Fact]
        public void TestRoundUpToDeciHourFilter()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_nonpad.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            
            var filter = new RoundUpToDeciHourFilter();
            test = filter.Filter(test);
            
            Assert.Equal(new TimeSpan(1, 18, 0), test.Days[0].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(5, 54, 0), test.Days[0].TotalTimeSpent);
            Assert.Equal(new TimeSpan(4, 36, 0), test.Tags[4].TotalTimeSpent);
            
            Assert.Equal(new TimeSpan(31, 24, 0), test.TotalTimeSpent);
        }
    }
}