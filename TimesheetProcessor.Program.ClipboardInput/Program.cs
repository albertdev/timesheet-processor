using System;
using System.IO;
using System.Linq;
using System.Windows;
using TimesheetProcessor.Core;
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

            Timesheet sheet;
            using (var reader = new StringReader(text))
            {
                sheet = new ManicTimeParser().ParseTimesheet(reader);
            }

            var outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var outputFilesPrefix = $"ManicTime_Timesheet_{sheet.Days.First().Day.Year}-w{sheet.WeekNumber}_{DateTime.Now:MM-dd}_";

            // Append most recent input to a file for later retrieval
            var normalizedText = text.Replace("\r\n", "\n").Replace("\n", "\r\n");
            File.AppendAllLines(Path.Combine(outputFolder, $"{outputFilesPrefix}inputlog.txt"), new []{"", $"Input on {DateTime.Now:s}", "", normalizedText});
            normalizedText = null;
            text = null;

            var roundedUp = new RoundToNearestDeciHourFilter().Filter(sheet);

            double expectedHoursToScale = (roundedUp.ExpectedHoursSpent - roundedUp.TotalTimeSpentWithReadonlyFlag).TotalSeconds;
            double actualHoursToScale = (roundedUp.TotalTimeSpent - roundedUp.TotalTimeSpentWithReadonlyFlag).TotalSeconds;
            var scalingFactor = expectedHoursToScale / actualHoursToScale;

            // Do not scale timesheet if there's only a one-hour difference (or even overtime). By that point it's better to review things manually.
            bool shouldScale = (actualHoursToScale < (expectedHoursToScale - 1));

            Timesheet result = sheet;
            Timesheet difference = null;
            if (shouldScale)
            {
                result = new ScaleTimesheetFilter(scalingFactor).Filter(sheet);
            }

            // Even if we don't scale we can better make sure the timesheet is rounded to 6 minute blocks
            result = new RoundToNearestDeciHourFilter().Filter(result);

            if (shouldScale)
            {
                difference = new SubtractTimesheetFilter(sheet).Filter(result);
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

                if (shouldScale)
                {
                    // Include 'difference' timesheet for applying manual corrections in ManicTime
                    writer.WriteLine();
                    writer.WriteLine();
                    new TabBasedTimesheetWriter().WriteTimesheet(difference, writer, false);
                }

                writer.Flush();
            }

            using (var writer = new StreamWriter(Path.Combine(outputFolder, $"{outputFilesPrefix}sample.csv")))
            {
                // Write (potentially corrected) timesheet once more, but now in format for import in external time tracking tool
                new ManicTimesheetWriter().WriteTimesheet(result, writer, true);
            }

            Console.Out.WriteLine("Processing complete");
            Console.ReadKey();
        }
    }
}
