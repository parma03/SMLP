using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Psmp.Application.Services.BackgroundJob.Methods;

namespace Psmp.Infrastructure.BackgroundJob.Hangfire;
public static class DataBackgroundJobs
{
    public static async Task ApplyDataBackgroundJobsAsync(this IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var jobOptions = configuration.GetSection(HangfireBackgroundJobOptions.SectionKey).Get<HangfireBackgroundJobOptions>();

        if (jobOptions.IsJobRun)
        {
            var paymentBacgroundJob = serviceProvider.GetRequiredService<PaymentBackgroundJob>();
            await paymentBacgroundJob.PaymentBackgroundJobAsync();

            var inovationProjectUpdateMentorJob = serviceProvider.GetRequiredService<InovationProjectUpdateMentorJob>();
            await inovationProjectUpdateMentorJob.InovationProjectUpdateMentorJobAsync();

            var masterDataStatusBackgroundJob = serviceProvider.GetRequiredService<MasterDataStatusBackgroundJob>();
            await masterDataStatusBackgroundJob.MasterDataStatusBackgroundJobAsync();
        }
    }
}
