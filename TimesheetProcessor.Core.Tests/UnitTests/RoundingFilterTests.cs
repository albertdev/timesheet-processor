using System;
using System.Linq;
using TimesheetProcessor.Core.Filter;
using Xunit;

namespace TimesheetProcessor.Core.Tests.UnitTests
{
    public class RoundingFilterTests
    {
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
    }
}