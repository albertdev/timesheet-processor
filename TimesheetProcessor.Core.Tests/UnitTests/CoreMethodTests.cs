using System;
using Xunit;

namespace TimesheetProcessor.Core.Tests.UnitTests
{
    public class CoreMethodTests
    {
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

            Assert.Equal(TimeSpan.FromHours(40), timesheet.ExpectedHoursSpent);
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

            Assert.Equal(TimeSpan.FromHours(8 + 8 + 8), timesheet.ExpectedHoursSpent);
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
    }
}