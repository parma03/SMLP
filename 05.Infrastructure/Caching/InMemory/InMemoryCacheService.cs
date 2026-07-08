using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Smlp.Application.Services.Caching;

namespace Smlp.Infrastructure.Caching.InMemory;

/// <summary>
/// Implementasi ICacheService di atas IMemoryCache bawaan ASP.NET Core.
/// Cocok untuk single-instance deployment / development lokal tanpa Redis.
/// Key yang pernah di-set dilacak di _trackedKeys supaya RemoveByPrefixAsync
/// bisa bekerja (IMemoryCache sendiri tidak punya API untuk enumerasi key).
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private static readonly ConcurrentDictionary<string, byte> TrackedKeys = new();
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

    public InMemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue<T>(key, out var value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
        };
        options.RegisterPostEvictionCallback((evictedKey, _, _, _) => TrackedKeys.TryRemove((string)evictedKey, out _));

        _cache.Set(key, value, options);
        TrackedKeys[key] = 0;

        return Task.CompletedTask;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        var created = await factory(cancellationToken);
        await SetAsync(key, created, expiration, cancellationToken);
        return created;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        TrackedKeys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var keysToRemove = TrackedKeys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            TrackedKeys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
