﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using TimesheetProcessor.Core.Dto;
using TimesheetProcessor.Core.Filter;
using TimesheetProcessor.Core.Io;

namespace TimesheetProcessor.Program.ClipboardFormat
{
    /// <summary>
    /// This program reads a timesheet from the clipboard (in hour:minutes:second notation), check its syntax and then sum up all the totals.
    /// Optionally it can round to the nearest 6 minute interval or add zeroes for all the missing days.
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

            Timesheet result = sheet;
            if (args.Any(x => x.Equals("--round", StringComparison.InvariantCultureIgnoreCase)))
            {
                result = new RoundToNearestDeciHourFilter().Filter(result);
            }

            if (args.Any(x => x.Equals("--week", StringComparison.InvariantCultureIgnoreCase)))
            {
                // Makes sure timesheet contains all 7 days of the week for importing it in other time tracking tool
                result = new FullWeekFilter().Filter(result);
            }

            using (var writer = new StringWriter())
            {
                new TabBasedTimesheetWriter().WriteTimesheet(result, writer, true);
                writer.Flush();
                writer.Close();

                Clipboard.SetText(writer.ToString());
            }

            Console.Out.WriteLine("Clipboard modified");
            Console.ReadKey();
        }
    }
}
