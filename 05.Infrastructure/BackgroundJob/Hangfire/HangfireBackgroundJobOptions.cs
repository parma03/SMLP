namespace Smlp.Infrastructure.BackgroundJob.Hangfire;

public class HangfireBackgroundJobOptions
{
    public static readonly string SectionKey = $"{nameof(BackgroundJob)}:{nameof(Hangfire)}";

    public int WorkerCount { get; set; }
    public bool IsJobRun { get; set; }
    public Storage Storage { get; set; } = default!;
    public Dashboard Dashboard { get; set; } = default!;
}

public class Storage
{
    public string Provider { get; set; } = HangfireBackgroundJobStorageProvider.SqlServer;
}

public static class HangfireBackgroundJobStorageProvider
{
    public const string SqlServer = nameof(SqlServer);
    public const string MySql = nameof(MySql);
    public const string VaultSqlServer = nameof(VaultSqlServer);
}

public class Dashboard
{
    public string Url { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}
