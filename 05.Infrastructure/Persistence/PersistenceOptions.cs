namespace Psmp.Infrastructure.Persistence;

public class PersistenceOptions
{
    public const string SectionKey = nameof(Persistence);

    public string Provider { get; set; } = PersistenceProvider.None;
    public bool EnableSeeding { get; set; }
}

public static class PersistenceProvider
{
    public const string None = nameof(None);
    public const string SqlServer = nameof(SqlServer);
    public const string MySql = nameof(MySql);
    public const string VaultSqlServer = nameof(VaultSqlServer);
}
