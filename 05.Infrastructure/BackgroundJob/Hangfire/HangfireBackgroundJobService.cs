using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Smlp.Application.Services.BackgroundJob;

using HangfireBackgroundJob = Hangfire.BackgroundJob;
using HangfireRecurringJob = Hangfire.RecurringJob;

namespace Smlp.Infrastructure.BackgroundJob.Hangfire;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly ILogger<HangfireBackgroundJobService> _logger;

    public HangfireBackgroundJobService(ILogger<HangfireBackgroundJobService> logger)
    {
        _logger = logger;
    }

    public Task<string> RunJob(Expression<Func<Task>> methodCall)
    {
        var jobId = string.Empty;

        try
        {
            jobId = HangfireBackgroundJob.Enqueue(methodCall);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error in executing method {MethodName}", nameof(RunJob));

            throw;
        }

        return Task.FromResult(jobId);
    }

    public Task<string> AddScheduledJob(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        var jobId = string.Empty;

        try
        {
            jobId = HangfireBackgroundJob.Schedule(methodCall, enqueueAt);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error in executing method {MethodName}", nameof(AddScheduledJob));

            throw;
        }

        return Task.FromResult(jobId);
    }

    public Task<bool> RemoveScheduledJob(string scheduledJobId)
    {
        try
        {
            HangfireBackgroundJob.Delete(scheduledJobId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error in executing method {MethodName} with ID {ScheduledJobId}", nameof(RemoveScheduledJob), scheduledJobId);

            throw;
        }

        return Task.FromResult(true);
    }

    public Task<bool> AddRecurringJob(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = default!)
    {
        if (timeZone is null)
        {
            timeZone = TimeZoneInfo.Local;
        }

        try
        {
            HangfireRecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error in executing method {MethodName} with ID {RecurringJobId}", nameof(AddRecurringJob), recurringJobId);

            throw;
        }

        return Task.FromResult(true);
    }

    public Task<bool> UpdateRecurringJob(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = default!)
    {
        if (timeZone is null)
        {
            timeZone = TimeZoneInfo.Local;
        }

        try
        {
            HangfireRecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error in executing method {MethodName} with ID {RecurringJobId}", nameof(UpdateRecurringJob), recurringJobId);

            throw;
        }

        return Task.FromResult(true);
    }

    public Task<bool> RemoveRecurringJob(string recurringJobId)
    {
        try
        {
            HangfireRecurringJob.RemoveIfExists(recurringJobId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error in executing method {MethodName} with ID {RecurringJobId}", nameof(RemoveRecurringJob), recurringJobId);

            throw;
        }

        return Task.FromResult(true);
    }

    public Task<bool> AddContinuationJob(string parentJobId, Expression<Func<Task>> methodCall)
    {
        try
        {
            HangfireBackgroundJob.ContinueJobWith(parentJobId, methodCall);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error in executing method {MethodName} with Parent Job ID {ParentJobId}", nameof(AddContinuationJob), parentJobId);

            throw;
        }

        return Task.FromResult(true);
    }
}
