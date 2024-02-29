using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Io
{
    /// <summary>
    /// Prints a timesheet with "boxing" characters. Requires a mono-spaced font to view. The time is hour:minutes:seconds format.
    /// </summary>
    public class PrettyPrintTimesheetWriter
    {
        public PrettyPrintTimesheetWriter()
        {
        }

        public void WriteTimesheet(Timesheet sheet, TextWriter writer)
        {
            BuildTableRowsAndCells(sheet, out var columnWidths, out var header, out var lines, out var totals);

            WriteDividingLine(writer, columnWidths);
            WriteTableRow(writer, header, columnWidths);
            WriteDividingLine(writer, columnWidths);

            foreach (var line in lines)
            {
                WriteTableRow(writer, line, columnWidths);
            }
            WriteDividingLine(writer, columnWidths);
            WriteTableRow(writer, totals, columnWidths);
            WriteDividingLine(writer, columnWidths);
        }

        private void WriteDividingLine(TextWriter writer, int[] columnWidths)
        {
            writer.Write('+');
            
            for (int i = 0; i < columnWidths.Length; i++)
            {
                // For each column we should write 2 extra characters representing padding spaces on begin and end
                for (int c = 0; c < columnWidths[i] + 2; c++)
                {
                    writer.Write('-');
                }

                writer.Write('+');
            }
            writer.WriteLine();
        }

        private void WriteTableRow(TextWriter writer, TableRow row, int[] columnWidths)
        {
            var nLines = row.Height;
            // Generate strings with number of spaces to fill an empty cell
            var emptyCells = columnWidths.Select(w => String.Empty.PadLeft(w)).ToArray();

            for (int l = 0; l < nLines; l++)
            {
                writer.Write('|');
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    
                    writer.Write(' ');
                    var cell = row.Cells[i];
                    if (l < cell.Height)
                    {
                        int width = columnWidths[i];
                        string content = cell.Contents[l];
                        if (cell.PadLeft)
                        {
                            writer.Write(content.PadLeft(width));
                        }
                        else
                        {
                            writer.Write(content.PadRight(width));
                        }
                    }
                    else
                    {
                        writer.Write(emptyCells[i]);
                    }

                    writer.Write(' ');
                    writer.Write('|');
                }
                writer.WriteLine();
            }
        }

        private void BuildTableRowsAndCells(Timesheet sheet, out int[] columnWidths, out TableRow header, out TableRow[] lines, out TableRow totals)
        {
            var numberOfTags = sheet.TagLevels;
            var headerCells = new List<TableCell>();
            headerCells.AddRange(Enumerable.Range(1, numberOfTags).Select(i => new TableCell($"Tag {i}")));
            headerCells.Add(new TableCell("Notes"));
            headerCells.AddRange(Enumerable.Range(0, sheet.Days.Count).Select(i => new TableCell(sheet.Days[i].ToString())));
            headerCells.Add(new TableCell("Total"));
            header = new TableRow(headerCells);

            var allRows = new List<TableRow>();

            foreach (var tag in sheet.Tags)
            {
                var rowContents = new List<TableCell>();

                string[] tagIds = tag.TagIds;
                rowContents.AddRange(Enumerable.Range(0, numberOfTags)
                                        .Select(i => tag.TagIds.Length >= numberOfTags ? tag.TagIds[i] : "")
                                        .Select(tagId => new TableCell(tagId)));
                rowContents.Add(new TableCell(tag.Notes));

                // This makes sure that something gets written when the tag details entries are somehow incorrect
                foreach (var day in sheet.Days)
                {
                    var entry = tag.Entries.FirstOrDefault(x => x.Day.Equals(day));
                    if (entry == null)
                    {
                        rowContents.Add(new TableCell(ConvertTime(TimeSpan.Zero), true));
                    }
                    else
                    {
                        rowContents.Add(new TableCell(ConvertTime(entry.TimeSpent), true));
                    }
                }
                rowContents.Add(new TableCell(ConvertTime(tag.TotalTimeSpent), true));
                allRows.Add(new TableRow(rowContents));
            }

            lines = allRows.ToArray();

            var totalsContents = new List<TableCell>();
            // Use the first tag column to insert "Total" legend, other tag columns / notes are blanks
            totalsContents.Add(new TableCell("Total"));
            totalsContents.AddRange(Enumerable.Range(0, numberOfTags - 1).Select(i => new TableCell("")));
            totalsContents.Add(new TableCell(""));
            totalsContents.AddRange(Enumerable.Range(0, sheet.Days.Count)
                .Select(i => new TableCell(ConvertTime(sheet.Days[i].TotalTimeSpent), true)));
            totalsContents.Add(new TableCell(ConvertTime(sheet.TotalTimeSpent), true));
            totals = new TableRow(totalsContents);

            columnWidths = CalculateColumnWidths(header, lines, totals);
        }

        private int[] CalculateColumnWidths(TableRow header, TableRow[] lines, TableRow totals)
        {
            int MergeMaximum(int maxWidth, int cellWidth)
            {
                return Math.Max(maxWidth, cellWidth);
            }

            int[] currentMaximumWidth = new int[header.Cells.Length];

            currentMaximumWidth = currentMaximumWidth.Zip(header.Cells.Select(c => c.Width), MergeMaximum).ToArray();
            foreach (var line in lines)
            {
                currentMaximumWidth = currentMaximumWidth.Zip(line.Cells.Select(c => c.Width), MergeMaximum).ToArray();
            }
            currentMaximumWidth = currentMaximumWidth.Zip(totals.Cells.Select(c => c.Width), MergeMaximum).ToArray();
            return currentMaximumWidth;
        }

        private string ConvertTime(TimeSpan duration)
        {
            // Manually build formatted string, TimeSpan format strings roll over to days if more than 24 hours have passed
            if (duration.Ticks > TimeSpan.TicksPerDay)
            {
                // Truncate remainder, as those will be pulled from dedicated properties
                int hours = (int) duration.TotalHours;

                // Include a single quote, otherwise tools like Excel will still mangle it
                return $"{hours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            }

            // Rare negative duration. Can happen if rounding and other calculations somehow end up with a smaller target number.
            if (duration.Ticks < 0)
            {
                return duration.Duration().ToString("\\-h\\:mm\\:ss");
            }
            return duration.ToString("h\\:mm\\:ss", CultureInfo.InvariantCulture);
        }

        private class TableCell
        {
            public string[] Contents { get; }

            /// <summary>
            /// Max number of characters on lines in this cell.
            /// </summary>
            public int Width { get; }
            /// <summary>
            /// Number of lines in this cell.
            /// </summary>
            public int Height { get; }
            /// <summary>
            /// Whether the cell contents are right-aligned, in which case there will be spaces added on the left.
            /// </summary>
            public bool PadLeft { get; }

            public TableCell(string contents, bool padLeft = false)
            {
                PadLeft = padLeft;
                string[] lines = contents.Replace("\r\n", "\n").Split('\n');

                Height = lines.Length;
                Width = lines.Max(l => l.Length);
                Contents = lines;
            }
        }

        private class TableRow
        {
            public TableCell[] Cells { get; }

            public int Height { get; }

            public TableRow(IEnumerable<TableCell> cells)
            {
                Cells = cells.ToArray();
                Height = Cells.Max(c => c.Height);
            }
        }
    }
}