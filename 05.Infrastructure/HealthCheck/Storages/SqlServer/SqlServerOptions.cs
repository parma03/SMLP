namespace Psmp.Infrastructure.HealthCheck.Storages.SqlServer;

public class SqlServerOptions
{
    public static readonly string SectionKey = $"{nameof(HealthCheck)}:{nameof(HealthCheckOptions.UI)}:{nameof(HealthCheckOptions.UI.Storage)}:{nameof(SqlServer)}";

    public string ConnectionString { get; set; } = default!;
}
