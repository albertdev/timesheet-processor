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
        public void Test1()
        {
            Timesheet test;
            ManicTimeParser parser = new ManicTimeParser();
            using (var stream = new StreamReader("Testfiles/Timesheet_nonpad.tsv"))
            {
                test = parser.ParseTimesheet(stream);
            }
            Assert.Equal(7, test.Days.Count);
        }
    }
}