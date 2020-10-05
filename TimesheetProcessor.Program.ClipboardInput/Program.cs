using System;
using System.IO;
using System.Windows;
using TimesheetProcessor.Core;
using TimesheetProcessor.Core.Dto;
using TimesheetProcessor.Core.Filter;
using TimesheetProcessor.Core.Io;

namespace TimesheetProcessor.Program.ClipboardInput
{
    internal class Program
    {
        // Needs Single-Threaded Apartment to be able to use clipboard
        [STAThread]
        public static void Main(string[] args)
        {
            var text = Clipboard.GetText();

            if (text.StartsWith("Processed"))
            {
                Console.Error.WriteLine("Nothing to do, clipboard contains processed data");
                Console.ReadKey();
                return;
            }
            
            Timesheet sheet;
            using (var reader = new StringReader(text))
            {
                sheet = new ManicTimeParser().ParseTimesheet(reader);
            }
            
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

            string resultOutput;
            using (var writer = new StringWriter())
            {
                writer.WriteLine("Processed");
                writer.Write("Week:\t");
                writer.WriteLine(sheet.WeekNumber);
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
                writer.WriteLine();
                writer.WriteLine();
                // Print (potentially corrected) timesheet once more, but now in format for import in external time tracking tool
                new ManicTimesheetWriter().WriteTimesheet(result, writer, true);
                
                writer.Flush();
                resultOutput = writer.ToString();
            }

            Clipboard.SetText(resultOutput);
            Console.Out.WriteLine("Processing complete");
            Console.ReadKey();
        }
    }
}