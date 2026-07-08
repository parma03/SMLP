using System.Text.Json;
using Microsoft.Extensions.Options;
using Smlp.Application.Services.Caching;
using StackExchange.Redis;

namespace Smlp.Infrastructure.Caching.Redis;

/// <summary>
/// Implementasi ICacheService di atas Redis (StackExchange.Redis).
/// Dipakai saat Caching:Provider = "Redis", cocok untuk deployment
/// multi-instance di mana cache harus dibagi antar-server.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IDatabase _database;
    private readonly RedisCacheOptions _options;

    public RedisCacheService(IConnectionMultiplexer multiplexer, IOptions<RedisCacheOptions> options)
    {
        _multiplexer = multiplexer;
        _database = multiplexer.GetDatabase();
        _options = options.Value;
    }

    private string Prefixed(string key) => $"{_options.InstanceName}{key}";

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(Prefixed(key));
        if (!value.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(
            Prefixed(key),
            serialized,
            expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes));
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var created = await factory(cancellationToken);
        await SetAsync(key, created, expiration, cancellationToken);
        return created;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => _database.KeyDeleteAsync(Prefixed(key));

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var fullPrefix = Prefixed(prefix);

        foreach (var endpoint in _multiplexer.GetEndPoints())
        {
            var server = _multiplexer.GetServer(endpoint);

            await foreach (var redisKey in server.KeysAsync(pattern: $"{fullPrefix}*"))
            {
                await _database.KeyDeleteAsync(redisKey);
            }
        }
    }
}
