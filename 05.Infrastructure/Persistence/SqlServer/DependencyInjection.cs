/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Psmp.Application.Services.Persistence;
using Psmp.Infrastructure.Persistence.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerPersistenceService(this IServiceCollection services, SqlServerOptions sqlServerOptions, IHealthChecksBuilder healthChecksBuilder)
    {
        var migrationsAssembly = typeof(SqlServerSMLPDbContext).Assembly.FullName;

        services.AddDbContext<SqlServerSMLPDbContext>(options =>
        {
            options.UseSqlServer(sqlServerOptions.ConnectionString, builder =>
            {
                builder.MigrationsAssembly(migrationsAssembly);
                builder.MigrationsHistoryTable(TableNameFor.EfMigrationsHistory, nameof(PSMP));
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            options.ConfigureWarnings(wcb => wcb.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
            options.ConfigureWarnings(wcb => wcb.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        });

        services.AddScoped<IPsmpDbContext>(provider => provider.GetRequiredService<SqlServerSMLPDbContext>());

        healthChecksBuilder.AddSqlServer(
            connectionString: sqlServerOptions.ConnectionString,
            name: $"{nameof(Persistence)} {nameof(PersistenceOptions.Provider)} ({nameof(SqlServer)})");

        return services;
    }
}
