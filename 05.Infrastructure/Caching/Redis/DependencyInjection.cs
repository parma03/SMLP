using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Caching;
using Smlp.Shared.Common.Extensions;
using StackExchange.Redis;

namespace Smlp.Infrastructure.Caching.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddRedisCachingService(this IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        var redisOptions = configuration.GetSection(RedisCacheOptions.SectionKey).Get<RedisCacheOptions>() ?? new RedisCacheOptions();
        services.Configure<RedisCacheOptions>(configuration.GetSection(RedisCacheOptions.SectionKey));

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
        services.AddSingleton<ICacheService, RedisCacheService>();

        healthChecksBuilder.AddRedis(
            redisOptions.ConnectionString,
            name: $"{nameof(Caching).SplitWords()} Service ({nameof(Redis)})");

        return services;
    }
}
