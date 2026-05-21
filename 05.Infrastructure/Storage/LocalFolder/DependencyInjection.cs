using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Storage;

namespace Smlp.Infrastructure.Storage.LocalFolder;

public static class DependencyInjection
{
    public static IServiceCollection AddLocalFolderStorage(this IServiceCollection services)
    {
        services.AddScoped<IStorageService, LocalFolderStorageService>();
        return services;
    }
}
