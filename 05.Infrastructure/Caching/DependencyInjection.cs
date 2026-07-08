using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Infrastructure.Caching.InMemory;
using Smlp.Infrastructure.Caching.Redis;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;

namespace Smlp.Infrastructure.Caching;

public static class DependencyInjection
{
    public static IServiceCollection AddCachingService(this IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        var cachingOptions = configuration.GetSection(CachingOptions.SectionKey).Get<CachingOptions>() ?? new CachingOptions();

        switch (cachingOptions.Provider)
        {
            case CachingProvider.InMemory:
                services.AddInMemoryCachingService();
                break;

            case CachingProvider.Redis:
                services.AddRedisCachingService(configuration, healthChecksBuilder);
                break;

            default:
                throw new ArgumentException($"{CommonDisplayTextFor.Unsupported} {nameof(Caching).SplitWords()} {nameof(CachingOptions.Provider)}: {cachingOptions.Provider}");
        }

        return services;
    }
}
