namespace Smlp.Bsui.Features.Selections.Psychotest.Constants;

public static class RouteFor
{
    public static readonly string Selection = $"{nameof(Selection)}";
    public static readonly string Index = $"{nameof(Selection)}";
    public static readonly string Psychotest = $"{nameof(Psychotest)}";
    public static readonly string Scheduled = $"{Selection}/{nameof(Psychotest)}/{nameof(Schedule)}";
    public static readonly string ScheduleClosed = $"{Selection}/{nameof(Psychotest)}/{nameof(ScheduleClose)}";
    public static readonly string ScheduledDetails = $"{Selection}/{nameof(Psychotest)}/{nameof(ScheduleDetails)}";

    public static string Schedule(int type, Guid id)
    {
        return $"{Scheduled}/{type}/{id}";
    }
    public static string ScheduleClose(int type, Guid id)
    {
        return $"{ScheduleClosed}/{type}/{id}";
    }
    public static string ScheduleDetails(int type, Guid id)
    {
        return $"{ScheduledDetails}/{type}/{id}";
    }
    public static string Selections()
    {
        return $"{Index}";
    }
    public static string SelectionsClose()
    {
        return $"{Index}/Close";
    }
}
