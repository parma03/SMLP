namespace Smlp.Bsui.Services.Telemetry.ApplicationInsights;

public class ApplicationInsightsTelemetryOptions
{
    public static readonly string SectionKey = $"{nameof(Telemetry)}:{nameof(ApplicationInsights)}";

    public string ConnectionString { get; set; } = default!;
}
