using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Infrastructure.Storage.LocalFolder;
using Smlp.Infrastructure.Storage.None;

namespace Smlp.Infrastructure.Storage;

public static class DependencyInjection
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(StorageOptions.SectionKey).Get<StorageOptions>() ?? new StorageOptions();

        if (options.Provider == "LocalFolder")
        {
            services.Configure<LocalFolderStorageOptions>(configuration.GetSection(LocalFolderStorageOptions.SectionKey));
            services.AddLocalFolderStorage();
        }
        else
        {
            services.AddNoneStorage();
        }

        return services;
    }
}
