/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Persistence;
using Smlp.Infrastructure.Persistence.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerPersistenceService(this IServiceCollection services, SqlServerOptions sqlServerOptions, IHealthChecksBuilder healthChecksBuilder)
    {
        var migrationsAssembly = typeof(SqlServerSmlpDbContext).Assembly.FullName;

        services.AddDbContext<SqlServerSmlpDbContext>(options =>
        {
            options.UseSqlServer(sqlServerOptions.ConnectionString, builder =>
            {
                builder.MigrationsAssembly(migrationsAssembly);
                builder.MigrationsHistoryTable(TableNameFor.EfMigrationsHistory, "Smlp");
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            options.ConfigureWarnings(wcb => wcb.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
            options.ConfigureWarnings(wcb => wcb.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        });

        services.AddScoped<ISmlpDbContext>(provider => provider.GetRequiredService<SqlServerSmlpDbContext>());

        healthChecksBuilder.AddSqlServer(
            connectionString: sqlServerOptions.ConnectionString,
            name: $"{nameof(Persistence)} {nameof(PersistenceOptions.Provider)} ({nameof(SqlServer)})");

        return services;
    }
}
