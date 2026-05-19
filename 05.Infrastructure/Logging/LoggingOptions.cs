namespace Smlp.Infrastructure.Logging;

public class LoggingOptions
{
    public const string SectionKey = nameof(Logging);

    public string Provider { get; set; } = LoggingProvider.None;
}

public static class LoggingProvider
{
    public const string None = nameof(None);
    public const string Serilog = nameof(Serilog);
}
