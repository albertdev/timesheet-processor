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
            
            var roundedUp = new RoundUpToDeciHourFilter().Filter(sheet);

            double flexibleHours = (roundedUp.ExpectedHoursSpent - roundedUp.TotalTimeSpentWithReadonlyFlag).TotalSeconds;
            double hoursUpForScaling = (roundedUp.TotalTimeSpent - roundedUp.TotalTimeSpentWithReadonlyFlag).TotalSeconds;
            var scalingFactor = flexibleHours / hoursUpForScaling;
            
            var scaledTimesheet = new ScaleTimesheetFilter(scalingFactor).Filter(sheet);
            scaledTimesheet = new RoundToNearestDeciHourFilter().Filter(scaledTimesheet);
            
            var difference = new SubtractTimesheetFilter(sheet).Filter(scaledTimesheet);

            string result;
            using (var writer = new StringWriter())
            {
                writer.WriteLine("Processed");
                writer.Write("Week:\t");
                writer.WriteLine(sheet.WeekNumber);
                writer.Write("Scale factor:\t");
                writer.WriteLine(scalingFactor);
                writer.WriteLine();
                writer.WriteLine();
                
                // Should actually be a comma-separated one if we want to use this directly with helper scripts, but anyway
                new TabBasedTimesheetWriter().WriteTimesheet(scaledTimesheet, writer, true);

                writer.WriteLine();
                writer.WriteLine();
                new TabBasedTimesheetWriter().WriteTimesheet(difference, writer, false);
                
                writer.Flush();
                result = writer.ToString();
            }

            Clipboard.SetText(result);
            Console.Out.WriteLine("Processing complete");
            Console.ReadKey();
        }
    }
}