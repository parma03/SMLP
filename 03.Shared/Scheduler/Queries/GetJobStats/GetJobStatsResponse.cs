using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Scheduler.Queries.GetJobStats;

public class GetJobStatsResponse : Response
{
    public long Enqueued { get; set; }
    public long Processing { get; set; }
    public long Succeeded { get; set; }
    public long Failed { get; set; }
    public long Scheduled { get; set; }
    public long Recurring { get; set; }
    public int Servers { get; set; }
    public List<JobHistoryItem> RecentHistory { get; set; } = new();
}

public class JobHistoryItem
{
    public string JobId { get; set; } = default!;
    public string JobName { get; set; } = default!;
    public string State { get; set; } = default!;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public double? DurationMs { get; set; }
    public string? ExceptionMessage { get; set; }
}
