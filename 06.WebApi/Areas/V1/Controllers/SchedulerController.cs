using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.BackgroundJobs;
using Smlp.Application.Services.BackgroundJob;
using Smlp.Shared.Scheduler.Constants;
using Smlp.Shared.Scheduler.Queries.GetJobStats;
using Smlp.Shared.Scheduler.Queries.GetRecurringJobs;

namespace Smlp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize(Policy = "AdminOnly")]
public class SchedulerController : ApiControllerBase
{
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IServiceProvider _serviceProvider;

    public SchedulerController(IBackgroundJobService backgroundJobService, IServiceProvider serviceProvider)
    {
        _backgroundJobService = backgroundJobService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>Daftar semua recurring jobs yang terdaftar di Hangfire</summary>
    [HttpGet(ApiEndpoint.V1.Scheduler.RouteTemplateFor.RecurringJobs)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(GetRecurringJobsResponse))]
    public ActionResult<GetRecurringJobsResponse> GetRecurringJobs()
    {
        using var connection = JobStorage.Current.GetConnection();
        var recurringJobs = connection.GetRecurringJobs();

        var items = recurringJobs.Select(j => new RecurringJobItem
        {
            Id = j.Id,
            CronExpression = j.Cron,
            TimeZoneId = j.TimeZone?.Id ?? "UTC",
            NextExecution = j.NextExecution.HasValue
                ? new DateTimeOffset(j.NextExecution.Value, TimeSpan.Zero)
                : null,
            LastExecution = j.LastExecution.HasValue
                ? new DateTimeOffset(j.LastExecution.Value, TimeSpan.Zero)
                : null,
            LastJobState = j.LastJobState,
            Error = j.Error,
            IsEnabled = !j.Removed
        }).ToList();

        return Ok(new GetRecurringJobsResponse { Jobs = items });
    }

    /// <summary>Statistik job Hangfire: enqueued, processing, succeeded, failed</summary>
    [HttpGet(ApiEndpoint.V1.Scheduler.RouteTemplateFor.JobStats)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(GetJobStatsResponse))]
    public ActionResult<GetJobStatsResponse> GetJobStats()
    {
        var monitoring = JobStorage.Current.GetMonitoringApi();
        var stats = monitoring.GetStatistics();

        // Recent succeeded jobs (last 20)
        var succeeded = monitoring.SucceededJobs(0, 20);
        var failed = monitoring.FailedJobs(0, 10);

        var history = succeeded.Select(j => new JobHistoryItem
        {
            JobId = j.Key,
            JobName = j.Value?.Job?.Method?.Name ?? "Unknown",
            State = "Succeeded",
            StartedAt = j.Value?.SucceededAt?.ToLocalTime() ?? DateTimeOffset.MinValue,
            FinishedAt = j.Value?.SucceededAt?.ToLocalTime(),
            DurationMs = j.Value?.TotalDuration
        }).ToList();

        history.AddRange(failed.Select(j => new JobHistoryItem
        {
            JobId = j.Key,
            JobName = j.Value?.Job?.Method?.Name ?? "Unknown",
            State = "Failed",
            StartedAt = j.Value?.FailedAt?.ToLocalTime() ?? DateTimeOffset.MinValue,
            ExceptionMessage = j.Value?.ExceptionMessage
        }));

        var servers = monitoring.Servers();

        return Ok(new GetJobStatsResponse
        {
            Enqueued = stats.Enqueued,
            Processing = stats.Processing,
            Succeeded = stats.Succeeded,
            Failed = stats.Failed,
            Scheduled = stats.Scheduled,
            Recurring = stats.Recurring,
            Servers = servers.Count,
            RecentHistory = history.OrderByDescending(h => h.StartedAt).Take(20).ToList()
        });
    }

    /// <summary>Trigger manual sebuah recurring job</summary>
    [HttpPost(ApiEndpoint.V1.Scheduler.RouteTemplateFor.TriggerJob)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TriggerJob([FromRoute] string jobId)
    {
        using var connection = JobStorage.Current.GetConnection();
        var recurringJobs = connection.GetRecurringJobs();
        var job = recurringJobs.FirstOrDefault(j => j.Id == jobId);

        if (job is null)
            return NotFound($"Recurring job '{jobId}' tidak ditemukan.");

        // Trigger sesuai nama job yang dikenal
        if (jobId == "smlp.loyalty.expiredpoints")
        {
            var jobInstance = _serviceProvider.GetRequiredService<IExpiredPointsJob>();
            await _backgroundJobService.RunJob(() => jobInstance.RunAsync());
            return Ok(new { message = $"Job '{jobId}' berhasil di-trigger secara manual." });
        }

        // Generic trigger via RecurringJob.Trigger
        RecurringJob.TriggerJob(jobId);
        return Ok(new { message = $"Job '{jobId}' berhasil di-trigger." });
    }
}
