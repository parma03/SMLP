using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Caching;

namespace Smlp.Infrastructure.Caching.InMemory;

public static class DependencyInjection
{
    public static IServiceCollection AddInMemoryCachingService(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, InMemoryCacheService>();

        return services;
    }
}
