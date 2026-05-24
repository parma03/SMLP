using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.BackgroundJobs;
using Smlp.Application.Services.BackgroundJob;

namespace Smlp.Infrastructure.BackgroundJob.Hangfire;

public static class DataBackgroundJobs
{
    private const string ExpiredPointsJobId = "smlp.loyalty.expiredpoints";

    public static async Task ApplyDataBackgroundJobsAsync(this IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var jobOptions = configuration.GetSection(HangfireBackgroundJobOptions.SectionKey).Get<HangfireBackgroundJobOptions>();

        if (jobOptions?.IsJobRun == true)
        {
            var backgroundJobService = serviceProvider.GetRequiredService<IBackgroundJobService>();

            // ── Expired Points Scheduler ──────────────────────────────────────────
            // Jalankan setiap hari pukul 00:30 WIB (17:30 UTC)
            await backgroundJobService.AddRecurringJob(
                recurringJobId: ExpiredPointsJobId,
                methodCall: () => serviceProvider.GetRequiredService<IExpiredPointsJob>().RunAsync(),
                cronExpression: "30 17 * * *",  // 00:30 WIB = 17:30 UTC
                timeZone: TimeZoneInfo.Utc
            );
        }

        await Task.CompletedTask;
    }
}
