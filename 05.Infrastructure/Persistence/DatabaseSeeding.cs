using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Persistence.Seeders;

namespace Smlp.Infrastructure.Persistence;

public static class DatabaseSeeding
{
    public static async Task ApplyDatabaseSeedingAsync(this IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var persistenceOptions = configuration.GetSection(PersistenceOptions.SectionKey).Get<PersistenceOptions>();

        if (persistenceOptions!.EnableSeeding)
        {
            var userSeeder = serviceProvider.GetRequiredService<UserSeeder>();
            await userSeeder.SeedUsersAsync();
        }
    }
}
