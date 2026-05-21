using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Infrastructure.Persistence.SqlServer;
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
                break;

            case PersistenceProvider.SqlServer:
                var sqlServerOptions = configuration.GetSection(SqlServerOptions.SectionKey).Get<SqlServerOptions>();
                services.AddSqlServerPersistenceService(sqlServerOptions!, healthChecksBuilder);
                break;

            default:
                throw new ArgumentException($"{CommonDisplayTextFor.Unsupported} {nameof(Persistence)} {nameof(PersistenceOptions.Provider)}: {persistenceOptions.Provider}");
        }

        return services;
    }
}
