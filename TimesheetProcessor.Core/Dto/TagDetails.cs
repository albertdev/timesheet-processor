using System.Collections.Generic;

namespace TimesheetProcessor.Core.Dto
{
    public class TagDetails
    {
        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public string Notes { get; set; }
        public IList<TimeEntry> TimeSpent { get; set; } = new List<TimeEntry>();

        public string TagId => $"{Tag1}, {Tag2}";
    }
}