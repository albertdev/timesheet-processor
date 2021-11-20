using System;
using System.Linq;
using TimesheetProcessor.Core.Filter;
using Xunit;

namespace TimesheetProcessor.Core.Tests.UnitTests
{
    public class ElasticFilterTests
    {
        [Fact]
        public void TestElasticDistribution()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:30:0").AddEntry("building, prod1", "1:0:0").AddEntry("elastic", "0:15:0")
                .ToTimesheet();

            var filter = new ElasticFilter();
            timesheet = filter.Filter(timesheet);

            // Check that Elastic tag is removed from resulting timesheet
            Assert.DoesNotContain(timesheet.Tags, details => details.Tag1.Equals("elastic", StringComparison.InvariantCultureIgnoreCase));

            Assert.Equal(new TimeSpan(0, 35, 0), timesheet.Days[0].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(1, 10, 0), timesheet.Days[0].Entries[1].TimeSpent);
            Assert.Equal(new TimeSpan(1, 45, 0), timesheet.Days[0].TotalTimeSpent);
        }

        [Fact]
        public void TestElasticEntryCannotBeCombinedWithReadonly()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddReadOnlyEntry("admin", "0:30:0").AddEntry("elastic", "0:15:0")
                .ToTimesheet();

            var filter = new ElasticFilter();
            var exception = Assert.Throws<Exception>(() => filter.Filter(timesheet));
            Assert.Contains("writeable", exception.Message);
        }

        [Fact]
        public void TestElasticEntryCannotBeAlone()
        {
            var timesheet = new Builder()
                .Day("2020-02-03").AddEntry("elastic", "0:15:0")
                .ToTimesheet();

            var filter = new ElasticFilter();
            var exception = Assert.Throws<Exception>(() => filter.Filter(timesheet));
            Assert.Contains("Elastic", exception.Message);
        }

        [Fact]
        public void TestOnlyWriteableEntriesAffected()
        {
            var timesheet = new Builder()
                .Day("2020-02-09").AddReadOnlyEntry("admin", "0:10:0").AddEntry("building, prod1", "0:10:0").AddEntry("elastic", "0:10:0")
                .ToTimesheet();

            var filter = new ElasticFilter();
            timesheet = filter.Filter(timesheet);

            Assert.Equal(new TimeSpan(0, 30, 0), timesheet.Days[0].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 10, 0), timesheet.Days[0].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(0, 20, 0), timesheet.Days[0].Entries[1].TimeSpent);
        }

        [Fact]
        public void TestCombinations()
        {
            var original = new Builder()
                .Day("2020-02-03").AddEntry("admin", "0:30:0").AddEntry("building, prod1", "1:0:0").AddEntry("elastic", "0:15:0")
                .AndOnDay("2020-02-04").AddEntry("inventing", "0:7:1").AddEntry("building, prod2", "0:9:0")
                .AndOnDay("2020-02-05").AddEntry("admin", "0:10:0").AddEntry("elastic", "0:10:0")
                .AndOnDay("2020-02-06").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-07").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-08").AddEntry("admin", "0:0:1")
                .AndOnDay("2020-02-09").AddReadOnlyEntry("admin", "0:10:0").AddEntry("building, prod1", "0:10:0").AddEntry("elastic", "0:10:0")
                .ToTimesheet();

            var filter = new ElasticFilter();
            var timesheet = filter.Filter(original);

            Assert.Equal(original.TotalTimeSpent, timesheet.TotalTimeSpent);

            Assert.Equal(new TimeSpan(0, 35, 0), timesheet.Days[0].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(1, 10, 0), timesheet.Days[0].Entries[1].TimeSpent);
            Assert.Equal(new TimeSpan(1, 45, 0), timesheet.Days[0].TotalTimeSpent);

            Assert.Equal(new TimeSpan(0, 20, 0), timesheet.Days[2].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 20, 0), timesheet.Days[2].Entries[0].TimeSpent);

            Assert.Equal(new TimeSpan(0, 30, 0), timesheet.Days[6].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0, 10, 0), timesheet.Days[6].Entries[0].TimeSpent);
            Assert.Equal(new TimeSpan(0, 20, 0), timesheet.Days[6].Entries[1].TimeSpent);

            // Other days where no Elastic tag was present should be unmodified
            Assert.Equal(new TimeSpan(0, 16, 1), timesheet.Days[1].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0,  0, 1), timesheet.Days[3].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0,  0, 1), timesheet.Days[4].TotalTimeSpent);
            Assert.Equal(new TimeSpan(0,  0, 1), timesheet.Days[5].TotalTimeSpent);
        }
    }
}