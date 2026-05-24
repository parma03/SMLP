using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Scheduler.Queries.GetRecurringJobs;

public class GetRecurringJobsResponse : Response
{
    public List<RecurringJobItem> Jobs { get; set; } = new();
}

public class RecurringJobItem
{
    public string Id { get; set; } = default!;
    public string CronExpression { get; set; } = default!;
    public string TimeZoneId { get; set; } = "UTC";
    public DateTimeOffset? NextExecution { get; set; }
    public DateTimeOffset? LastExecution { get; set; }
    public string? LastJobState { get; set; }
    public string? Error { get; set; }
    public bool IsEnabled { get; set; } = true;
}
