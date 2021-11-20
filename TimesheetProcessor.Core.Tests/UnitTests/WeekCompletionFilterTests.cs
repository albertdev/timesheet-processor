using System;
using TimesheetProcessor.Core.Filter;
using Xunit;

namespace TimesheetProcessor.Core.Tests.UnitTests
{
    public class WeekCompletionFilterTests
    {
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