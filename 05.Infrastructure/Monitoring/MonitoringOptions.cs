namespace Smlp.Infrastructure.Monitoring;

public class MonitoringOptions
{
    public const string SectionKey = nameof(Monitoring);

    /// <summary>Nama service yang muncul di Grafana/App Insights. Default fallback ke AppInfo:FullName bila kosong.</summary>
    public string ServiceName { get; set; } = default!;

    public PrometheusOptions Prometheus { get; set; } = default!;
    public ApplicationInsightsMonitoringOptions ApplicationInsights { get; set; } = default!;
}

public class PrometheusOptions
{
    public bool Enabled { get; set; }

    /// <summary>Path endpoint scrape, contoh: /metrics</summary>
    public string Endpoint { get; set; } = "/metrics";
}

public class ApplicationInsightsMonitoringOptions
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = default!;
}
