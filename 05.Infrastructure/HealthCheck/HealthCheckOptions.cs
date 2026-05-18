namespace Psmp.Infrastructure.HealthCheck;

public class HealthCheckOptions
{
    public const string SectionKey = nameof(HealthCheck);

    public string Endpoint { get; set; } = default!;
    public UI UI { get; set; } = default!;
}

public class UI
{
    public bool Enabled { get; set; }
    public string AbsoluteUri { get; set; } = default!;
    public Endpoints Endpoints { get; set; } = default!;
    public Storage Storage { get; set; } = default!;
}

public class Endpoints
{
    public string UI { get; set; } = default!;
    public string Api { get; set; } = default!;
}

public class Storage
{
    public string Provider { get; set; } = HealthCheckStorageProvider.SqlServer;
}

public static class HealthCheckStorageProvider
{
    public const string SqlServer = nameof(SqlServer);
    public const string MySql = nameof(MySql);
    public const string VaultSqlServer = nameof(VaultSqlServer);
}
