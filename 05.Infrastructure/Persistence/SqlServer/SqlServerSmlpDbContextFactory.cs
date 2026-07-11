/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Smlp.Infrastructure.Persistence.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Dipakai khusus oleh tooling `dotnet ef` (migrations, dsb.) supaya EF Core TIDAK perlu
/// membangun seluruh host aplikasi lewat Program.cs (AddInfrastructure, Hangfire, OpenTelemetry,
/// JWT, dst). Dengan factory ini, `dotnet ef` langsung dapat DbContextOptions tanpa menyentuh
/// service lain sama sekali — jadi migration tetap bisa dibuat/dijalankan walau ada service lain
/// yang belum siap (misalnya database belum ada, atau dependency eksternal belum jalan).
///
/// Connection string diambil dari appsettings milik startup project (--startup-project 06.WebApi),
/// karena `dotnet ef` menjalankan CreateDbContext dengan current directory = folder startup project.
/// </summary>
public class SqlServerSmlpDbContextFactory : IDesignTimeDbContextFactory<SqlServerSmlpDbContext>
{
    public SqlServerSmlpDbContext CreateDbContext(string[] args)
    {
        var environmentName =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Local";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration[$"{SqlServerOptions.SectionKey}:{nameof(SqlServerOptions.ConnectionString)}"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                $"Connection string tidak ditemukan. Pastikan '{SqlServerOptions.SectionKey}:{nameof(SqlServerOptions.ConnectionString)}' " +
                $"sudah diisi di appsettings.json / appsettings.{environmentName}.json pada 06.WebApi, " +
                "atau set environment variable ASPNETCORE_ENVIRONMENT / DOTNET_ENVIRONMENT sesuai profile yang dipakai.");

        var optionsBuilder = new DbContextOptionsBuilder<SqlServerSmlpDbContext>();

        optionsBuilder.UseSqlServer(connectionString, builder =>
        {
            builder.MigrationsAssembly(typeof(SqlServerSmlpDbContext).Assembly.FullName);
            builder.MigrationsHistoryTable(TableNameFor.EfMigrationsHistory, "Smlp");
            builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        optionsBuilder.ConfigureWarnings(wcb => wcb.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
        optionsBuilder.ConfigureWarnings(wcb => wcb.Throw(RelationalEventId.MultipleCollectionIncludeWarning));

        return new SqlServerSmlpDbContext(optionsBuilder.Options);
    }
}
