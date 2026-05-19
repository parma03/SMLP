namespace Smlp.Infrastructure.BackgroundJob.Hangfire.Storages.SqlServer;

public class SqlServerOptions
{
    public static readonly string SectionKey = $"{nameof(BackgroundJob)}:{nameof(Hangfire)}:{nameof(HangfireBackgroundJobOptions.Storage)}:{nameof(SqlServer)}";

    public string ConnectionString { get; set; } = default!;
}
