using System.IO;
using TimesheetProcessor.Core.Io;
using Xunit;

namespace TimesheetProcessor.Core.Tests.UnitTests
{
    public class SheetWriterTests
    {
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
    }
}