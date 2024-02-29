using System;
using System.IO;
using System.Linq;
using System.Windows;
using TimesheetProcessor.Core.Dto;
using TimesheetProcessor.Core.Filter;
using TimesheetProcessor.Core.Io;

namespace TimesheetProcessor.Program.ClipboardInput
{
    /// <summary>
    /// This program reads a timesheet from the clipboard (in hour:minutes:second notation) and will write several processed timesheet files to the
    /// user's Documents folder.
    /// </summary>
    internal class Program
    {
        // Needs Single-Threaded Apartment to be able to use clipboard
        [STAThread]
        public static void Main(string[] args)
        {
            var text = Clipboard.GetText();

            Timesheet inputSheet;
            using (var reader = new StringReader(text))
            {
                inputSheet = new ManicTimeParser().ParseTimesheet(reader);
            }

            var outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Planning");
            var outputFilesPrefix = $"ManicTime_Timesheet_{inputSheet.Days.First().Day.Year:0000}-w{inputSheet.WeekNumber:00}_{DateTime.Now:MM-dd}_";

            BackupMostRecentInput(outputFolder, outputFilesPrefix, text);

            // First distribute the "Elastic" time code across every writable entry in the sheet. This will form the basis of the result.
            var workingSheet = new ElasticFilter().Filter(inputSheet);

            var rounded = new RoundToNearestQuarterHourFilter().Filter(workingSheet);

            double expectedHoursToScale = (rounded.ExpectedHoursSpent - rounded.TotalTimeSpentWithReadonlyFlag).TotalHours;
            double actualHoursToScale = (rounded.TotalTimeSpent - rounded.TotalTimeSpentWithReadonlyFlag).TotalHours;
            var scalingFactor = expectedHoursToScale / actualHoursToScale;

            // Do not scale timesheet if there's only a one-hour difference (or even overtime). By that point it's better to review things manually.
            bool shouldScale = (actualHoursToScale < (expectedHoursToScale - 1));

            Timesheet result = workingSheet;
            Timesheet difference = null;
            if (shouldScale)
            {
                result = new ScaleTimesheetFilter(scalingFactor).Filter(workingSheet);
            }

            // Even if we don't scale we can better make sure the timesheet is quantized to time blocks
            result = new RoundToNearestQuarterHourFilter().Filter(result);

            if (shouldScale)
            {
                difference = new SubtractTimesheetFilter(inputSheet).Filter(result);
                difference = new FullWeekFilter().Filter(difference);
            }

            // Make sure timesheet contains all 7 days of the week for importing it in other time tracking tool
            result = new FullWeekFilter().Filter(result);

            using (var writer = new StreamWriter(Path.Combine(outputFolder, $"{outputFilesPrefix}output.txt")))
            {
                writer.Write("Scale factor:\t");

                if (shouldScale)
                {
                    writer.WriteLine(scalingFactor);
                }
                else
                {
                    writer.WriteLine("None");
                }
                writer.WriteLine();
                writer.WriteLine();

                new TabBasedTimesheetWriter().WriteTimesheet(result, writer, true);

                writer.WriteLine();
                writer.WriteLine();

                new PrettyPrintTimesheetWriter().WriteTimesheet(result, writer);

                if (shouldScale)
                {
                    // Include 'difference' timesheet to have an idea what corrections are suggested in ManicTime. Includes negative values
                    writer.WriteLine();
                    writer.WriteLine();
                    new PrettyPrintTimesheetWriter().WriteTimesheet(difference, writer);

                    // Write 'difference' timesheet once more but now turn negative values into zero timespans
                    writer.WriteLine();
                    writer.WriteLine();
                    new PrettyPrintTimesheetWriter().WriteTimesheet(new ClipToZeroFilter().Filter(difference), writer);
                }

                writer.Flush();
            }

            using (var writer = new StreamWriter(Path.Combine(outputFolder, $"{outputFilesPrefix}sample.csv")))
            {
                // Write (potentially corrected) timesheet once more, but now in format for import in external time tracking tool
                new ManicTimesheetWriter().WriteTimesheet(result, writer);
            }

            Console.Out.WriteLine("Processing complete");
            Console.ReadKey();
        }

        private static void BackupMostRecentInput(string outputFolder, string outputFilesPrefix, string inputSheetText)
        {
            var outputFile = Path.Combine(outputFolder, $"{outputFilesPrefix}inputlog.txt");
            var normalizedText = inputSheetText.Replace("\r\n", "\n").Replace("\n", "\r\n");
            File.AppendAllLines(outputFile, new[] {"", $"Input on {DateTime.Now:s}", "", normalizedText});
        }
    }
}
