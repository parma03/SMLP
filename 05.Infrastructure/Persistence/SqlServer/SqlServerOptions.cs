namespace Psmp.Infrastructure.Persistence.SqlServer;

public class SqlServerOptions
{
    public static readonly string SectionKey = $"{nameof(Persistence)}:{nameof(SqlServer)}";

    public string ConnectionString { get; set; } = default!;
}
