using System;
using System.IO;
using System.Text;
using TimesheetProcessor.Core.Dto;
using TimesheetProcessor.Core.Filter;
using TimesheetProcessor.Core.Io;

namespace TimesheetProcessor.Program.TsvDiff
{
    /// <summary>
    /// This program reads two timesheets from input TSV file (in hour:minutes:second notation) and will write the difference to a new TSV file.
    /// Immediately after that there's another copy but this time any negative values have been clipped to zero.
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

            Timesheet sheet1 = null;
            Timesheet sheet2 = null;
            Exception sheet1Exception = null;
            Exception sheet2Exception = null;

            try
            {
                using (var reader = new StreamReader(sheet1Path, Encoding.UTF8))
                {
                    sheet1 = new ManicTimeParser().ParseTimesheet(reader);
                }
            }
            catch (Exception e)
            {
                sheet1Exception = e;
            }

            try
            {
                using (var reader = new StreamReader(sheet2Path, Encoding.UTF8))
                {
                    sheet2 = new ManicTimeParser().ParseTimesheet(reader);
                }
            }
            catch (Exception e)
            {
                sheet2Exception = e;
            }

            if (sheet1Exception != null && sheet2Exception != null)
            {
                Console.Error.WriteLine($"Failed to parse both input files {sheet1Path} and {sheet2Path}");
                Console.Error.Write("Error 1: ");
                Console.Error.WriteLine(sheet1Exception.ToString());
                Console.Error.Write("Error 2: ");
                Console.Error.WriteLine(sheet2Exception.ToString());
                Environment.Exit(1);
            }
            else if (sheet1Exception != null)
            {
                throw new Exception($"Failed to parse input file {sheet1Path}", sheet1Exception);
            }
            else if (sheet2Exception != null)
            {
                throw new Exception($"Failed to parse input file {sheet2Path}", sheet2Exception);
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
