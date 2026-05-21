using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.BackgroundJob;
using Smlp.Infrastructure.BackgroundJob.Hangfire.Storages.SqlServer;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;

namespace Smlp.Infrastructure.BackgroundJob.Hangfire;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireBackgroundJobService(this IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        var hangfireBackgroundJobOptions = configuration.GetSection(HangfireBackgroundJobOptions.SectionKey).Get<HangfireBackgroundJobOptions>();

        switch (hangfireBackgroundJobOptions!.Storage.Provider)
        {
            case HangfireBackgroundJobStorageProvider.SqlServer:
                var sqlServerOptions = configuration.GetSection(SqlServerOptions.SectionKey).Get<SqlServerOptions>();
                services.AddHangfireUsingSqlServerDatabase(sqlServerOptions!, healthChecksBuilder);
                break;

            default:
                throw new ArgumentException($"{CommonDisplayTextFor.Unsupported} {nameof(Hangfire)} {nameof(BackgroundJob).SplitWords()} {nameof(HangfireBackgroundJobOptions.Storage)} {nameof(HangfireBackgroundJobOptions.Storage.Provider)}: {hangfireBackgroundJobOptions.Storage.Provider}");
        }

        services.AddTransient<IBackgroundJobService, HangfireBackgroundJobService>();
        services.AddHangfireServer(options => options.WorkerCount = hangfireBackgroundJobOptions.WorkerCount);

        healthChecksBuilder.AddHangfire(
            setup => setup.MinimumAvailableServers = 1,
            name: $"{nameof(BackgroundJob).SplitWords()} {CommonDisplayTextFor.Service} ({nameof(Hangfire)})");

        return services;
    }

    public static IApplicationBuilder UseHangfireBackgroundJobService(this IApplicationBuilder app, IConfiguration configuration)
    {
        var hangfireBackgroundJobOptions = configuration.GetSection(HangfireBackgroundJobOptions.SectionKey).Get<HangfireBackgroundJobOptions>();

        app.UseHangfireDashboard(hangfireBackgroundJobOptions!.Dashboard.Url, new DashboardOptions
        {
            Authorization = new[]
            {
                new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = hangfireBackgroundJobOptions.Dashboard.Username,
                            PasswordClear =  hangfireBackgroundJobOptions.Dashboard.Password
                        }
                    }
                })
            }
        });

        return app;
    }
}
