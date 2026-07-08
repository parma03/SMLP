namespace Smlp.Infrastructure.Caching;

public class CachingOptions
{
    public const string SectionKey = nameof(Caching);

    public string Provider { get; set; } = CachingProvider.InMemory;
}

public static class CachingProvider
{
    public const string InMemory = nameof(InMemory);
    public const string Redis = nameof(Redis);
}
