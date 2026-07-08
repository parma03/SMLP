namespace Smlp.Application.Services.Caching;

/// <summary>
/// Abstraksi caching yang provider-agnostic (InMemory / Redis).
/// Handler di Application layer bergantung pada interface ini, bukan
/// implementasi konkret, sehingga provider bisa diganti murni lewat konfigurasi.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ambil dari cache bila ada; bila tidak, jalankan <paramref name="factory"/>,
    /// simpan hasilnya ke cache, lalu kembalikan.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hapus semua entry yang key-nya diawali <paramref name="prefix"/>.
    /// Berguna untuk invalidasi cache per-fitur (mis. "dashboard:") tanpa
    /// perlu tahu key spesifik apa saja yang sedang tersimpan.
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
