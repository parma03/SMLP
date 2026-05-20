using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Smlp.Infrastructure.BackgroundJob.Hangfire;

public static class DataBackgroundJobs
{
    public static async Task ApplyDataBackgroundJobsAsync(this IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var jobOptions = configuration.GetSection(HangfireBackgroundJobOptions.SectionKey).Get<HangfireBackgroundJobOptions>();

        if (jobOptions?.IsJobRun == true)
        {
            // Add SMLP background jobs here as needed
            // e.g., tier recalculation, point expiry jobs
            await Task.CompletedTask;
        }
    }
}
