namespace Psmp.Bsui.Features.Applicants.Selection.Constants;

public static class TimelineStyleFor
{
    public static readonly string InProgress = "state-progress";
    public static readonly string Passed = "state-success";
    public static readonly string Completed = "completed";
    public static readonly string Intermediate = "intermediate";
    public static readonly string Rejected = nameof(Rejected);
    public static readonly string Failed = "state-failed";
}
