using System.Linq.Expressions;

namespace Smlp.Application.Services.BackgroundJob;

public interface IBackgroundJobService
{
    Task<string> RunJob(Expression<Func<Task>> methodCall);

    Task<string> AddScheduledJob(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);

    Task<bool> RemoveScheduledJob(string scheduledJobId);

    Task<bool> AddContinuationJob(string parentJobId, Expression<Func<Task>> methodCall);

    Task<bool> AddRecurringJob(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = default!);

    Task<bool> UpdateRecurringJob(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = default!);

    Task<bool> RemoveRecurringJob(string recurringJobId);
}
