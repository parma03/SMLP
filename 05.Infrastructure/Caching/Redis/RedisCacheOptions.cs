namespace Smlp.Infrastructure.Caching.Redis;

public class RedisCacheOptions
{
    public const string SectionKey = "Caching:Redis";

    /// <summary>
    /// StackExchange.Redis connection string, contoh: "localhost:6379,abortConnect=false".
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379,abortConnect=false";

    /// <summary>
    /// Prefix yang ditambahkan ke semua key, supaya beberapa aplikasi bisa
    /// berbagi satu Redis instance tanpa key collision.
    /// </summary>
    public string InstanceName { get; set; } = "smlp:";

    public int DefaultExpirationMinutes { get; set; } = 10;
}
