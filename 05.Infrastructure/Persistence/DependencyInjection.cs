using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Infrastructure.Persistence.None;
using Smlp.Infrastructure.Persistence.SqlServer;
using Smlp.Infrastructure.Persistence.VaultSqlServer;
using Smlp.Infrastructure.Vault;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceService(this IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        var persistenceOptions = configuration.GetSection(PersistenceOptions.SectionKey).Get<PersistenceOptions>();

        switch (persistenceOptions!.Provider)
        {
            case PersistenceProvider.None:
                services.AddNonePersistenceService();
                break;

            case PersistenceProvider.SqlServer:
                var sqlServerOptions = configuration.GetSection(SqlServerOptions.SectionKey).Get<SqlServerOptions>();
                services.AddSqlServerPersistenceService(sqlServerOptions!, healthChecksBuilder);
                break;
            case PersistenceProvider.VaultSqlServer:
                var vaultOptions = configuration.GetSection(VaultSqlServerOptions.SectionKey).Get<VaultSqlServerOptions>();
                var vaultClient = ClientConnect.UseAppRoleAuth(vaultOptions.URL, vaultOptions.RoleID, vaultOptions.SecretID, vaultOptions.Namespace);
                var vaultSqlServerSecret = StaticSecret.GetSecret(vaultClient, vaultOptions.SecretPath, vaultOptions.MountPoint);
                //Console.WriteLine($"Got vaultSqlServerSecret = {vaultSqlServerSecret.Data.Data["Server"]} " +
                //    $"{vaultSqlServerSecret.Data.Data["Catalog"]} " +
                //    $"{vaultSqlServerSecret.Data.Data["UserId"]} " +
                //    $"{vaultSqlServerSecret.Data.Data["Password"]}"); //remove after testing
                var vaultSqlServerOptions = new SqlServerOptions
                {
                    ConnectionString = $"Server={vaultSqlServerSecret.Data.Data["Server"]};" +
                                $"Database={vaultSqlServerSecret.Data.Data["Database"]};" +
                                $"user id={vaultSqlServerSecret.Data.Data["id"]};" +
                                $"pwd={vaultSqlServerSecret.Data.Data["pwd"]};" +
                                $"Trusted_Connection=False;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=Yes;"
                };
                services.AddSqlServerPersistenceService(vaultSqlServerOptions, healthChecksBuilder);
                break;
            default:
                throw new ArgumentException($"{CommonDisplayTextFor.Unsupported} {nameof(Persistence)} {nameof(PersistenceOptions.Provider)}: {persistenceOptions.Provider}");
        }

        return services;
    }
}
