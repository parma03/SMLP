namespace Smlp.Bsui.Services.Telemetry;

public class TelemetryOptions
{
    public const string SectionKey = nameof(Telemetry);

    public string Provider { get; set; } = TelemetryProvider.None;
}

public static class TelemetryProvider
{
    public const string None = nameof(None);
    public const string ApplicationInsights = nameof(ApplicationInsights);
}
