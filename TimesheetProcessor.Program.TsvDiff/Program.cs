using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using TimesheetProcessor.Core;
using TimesheetProcessor.Core.Dto;
using TimesheetProcessor.Core.Filter;
using TimesheetProcessor.Core.Io;

namespace TimesheetProcessor.Program.TsvDiff
{
    /// <summary>
    /// This program reads two timesheets from input TSV file (in hour:minutes:second notation) and will write the difference to a new TSV file.
    ///
    /// Operation: Sheet 1 - Sheet 2
    ///
    /// </summary>
    public static class Program
    {
        public static void Main(string[] args)
        {
            var dataFolder = Path.Combine(Environment.CurrentDirectory, "data");

            if (! Directory.Exists(dataFolder))
            {
                throw new Exception($"Path [{dataFolder}] not found");
            }

            var sheet1Path = Path.Combine(dataFolder, "sheet1.tsv");
            var sheet2Path = Path.Combine(dataFolder, "sheet2.tsv");

            if (! File.Exists(sheet1Path) || ! File.Exists(sheet2Path))
            {
                throw new Exception($"Input files of form 'sheetX.tsv' not found in folder {dataFolder}");
            }

            Timesheet sheet1;
            Timesheet sheet2;

            using (var reader = new StreamReader(sheet1Path, Encoding.UTF8))
            {
                sheet1 = new ManicTimeParser().ParseTimesheet(reader);
            }

            using (var reader = new StreamReader(sheet2Path, Encoding.UTF8))
            {
                sheet2 = new ManicTimeParser().ParseTimesheet(reader);
            }

            var difference = new SubtractTimesheetFilter(sheet2).Filter(sheet1);
            difference = new FullWeekFilter().Filter(difference);

            using (var writer = new StreamWriter(Path.Combine(dataFolder, "output.tsv"), false, Encoding.UTF8))
            {
                // Write result as-is (so with negative values)
                new TabBasedTimesheetWriter().WriteTimesheet(difference, writer, false);

                writer.WriteLine();
                writer.WriteLine();

                // Write result once more but now turn negative values into zero timespans
                new TabBasedTimesheetWriter().WriteTimesheet(new ClipToZeroFilter().Filter(difference), writer, false);

                writer.Flush();
            }

            Console.Out.WriteLine("Difference calculated");
            Console.ReadKey();
        }
    }
}
