using TimesheetProcessor.Core.Dto;

namespace TimesheetProcessor.Core.Filter
{
    public interface IFilter
    {
        Timesheet Filter(Timesheet original);
    }
}