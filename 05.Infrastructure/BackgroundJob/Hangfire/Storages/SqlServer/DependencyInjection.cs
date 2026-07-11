using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Smlp.Shared.Common.Extensions;

namespace Smlp.Infrastructure.BackgroundJob.Hangfire.Storages.SqlServer;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireUsingSqlServerDatabase(this IServiceCollection services, SqlServerOptions sqlServerOptions, IHealthChecksBuilder healthChecksBuilder)
    {
        services.AddHangfire(configuration =>
        {
            configuration.UseSerilogLogProvider();
            configuration.UseSqlServerStorage(sqlServerOptions.ConnectionString);
            configuration.UseSerializerSettings(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        });

        healthChecksBuilder.AddSqlServer(
            connectionString: sqlServerOptions.ConnectionString,
            name: $"{nameof(Hangfire)} {nameof(BackgroundJob).SplitWords()} {nameof(HangfireBackgroundJobOptions.Storage)} ({nameof(SqlServer)})");

        return services;
    }
}
