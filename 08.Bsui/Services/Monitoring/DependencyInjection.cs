using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Smlp.Bsui.Services.AppInfo;
using Smlp.Shared.Common.Constants;

namespace Smlp.Bsui.Services.Monitoring;

/// <summary>
/// Wiring OpenTelemetry METRICS untuk Bsui (Blazor Server). Sengaja hanya metrics
/// (bukan tracing) — request/dependency tracing untuk Bsui sudah dihandle oleh
/// Application Insights classic SDK di Services/Telemetry, jadi tidak perlu jalur
/// OTel tracing kedua yang bisa mengirim data duplikat ke resource App Insights yang sama.
/// Prometheus & Azure Monitor (metrics) di sini independen — bisa dinyalakan salah satu/keduanya.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddMonitoringService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MonitoringOptions>(configuration.GetSection(MonitoringOptions.SectionKey));

        var monitoringOptions = configuration.GetSection(MonitoringOptions.SectionKey).Get<MonitoringOptions>();
        var appInfoOptions = configuration.GetSection(AppInfoOptions.SectionKey).Get<AppInfoOptions>();

        if (monitoringOptions is null || (!monitoringOptions.Prometheus.Enabled && !monitoringOptions.ApplicationInsights.Enabled))
            return services;

        var serviceName = !string.IsNullOrWhiteSpace(monitoringOptions.ServiceName)
            ? monitoringOptions.ServiceName
            : appInfoOptions?.FullName ?? CommonValueFor.EntryAssemblySimpleName ?? "SMLP";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: serviceName,
                serviceVersion: CommonValueFor.EntryAssemblyVersion?.ToString() ?? "1.0.0"))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (monitoringOptions.Prometheus.Enabled)
                    metrics.AddPrometheusExporter();

                if (monitoringOptions.ApplicationInsights.Enabled)
                    metrics.AddAzureMonitorMetricExporter(o => o.ConnectionString = monitoringOptions.ApplicationInsights.ConnectionString);
            });

        return services;
    }

    /// <summary>
    /// Map endpoint scrape Prometheus. Dipanggil terpisah di Program.cs setelah UseRouting()
    /// (mis. sebaris dengan app.MapBlazorHub()), karena butuh IEndpointRouteBuilder.
    /// </summary>
    public static IEndpointRouteBuilder MapMonitoringEndpoints(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var monitoringOptions = configuration.GetSection(MonitoringOptions.SectionKey).Get<MonitoringOptions>();

        if (monitoringOptions is not null && monitoringOptions.Prometheus.Enabled)
        {
            endpoints.MapPrometheusScrapingEndpoint(monitoringOptions.Prometheus.Endpoint);
        }

        return endpoints;
    }
}
