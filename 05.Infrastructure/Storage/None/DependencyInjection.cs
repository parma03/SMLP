using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Storage;

namespace Smlp.Infrastructure.Storage.None;

public static class DependencyInjection
{
    public static IServiceCollection AddNoneStorage(this IServiceCollection services)
    {
        services.AddScoped<IStorageService, NoneStorageService>();
        return services;
    }
}
