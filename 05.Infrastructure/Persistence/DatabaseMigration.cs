using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smlp.Infrastructure.Persistence.SqlServer;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence;

public static class DatabaseMigration
{
    public static async Task ApplyDatabaseMigrationAsync<T>(this IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceProvider.GetRequiredService<ILogger<T>>();
        var persistenceOptions = configuration.GetSection(PersistenceOptions.SectionKey).Get<PersistenceOptions>();

        SmlpDbContext context;

        switch (persistenceOptions!.Provider)
        {
            case PersistenceProvider.None:
                logger.LogWarning($"No {nameof(Persistence)} {CommonDisplayTextFor.Service} is implemented.");
                break;

            case PersistenceProvider.SqlServer:
            case PersistenceProvider.VaultSqlServer:
                context = serviceProvider.GetRequiredService<SqlServerSmlpDbContext>();
                await MigrateSqlServerAsync(context, logger, persistenceOptions.Provider);
                break;

            default:
                throw new ArgumentException($"{CommonDisplayTextFor.Unsupported} {nameof(Persistence)} {nameof(PersistenceOptions.Provider)}: {persistenceOptions.Provider}");
        }
    }

    private static async Task MigrateSqlServerAsync<T>(SmlpDbContext context, ILogger<T> logger, string provider)
    {
        try
        {
            logger.LogInformation("Checking {PersistenceProvider} database and applying pending migrations if needed...", provider);
            await context.Database.MigrateAsync();
            logger.LogInformation("{PersistenceProvider} database is up to date.", provider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to migrate {PersistenceProvider} database. Verify the connection string, that the SQL " +
                "Server/LocalDB instance is running and reachable, and that the configured login has permission " +
                "to create/access the target database.",
                provider);
            throw;
        }
    }
}
