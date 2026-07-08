using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Smlp.Infrastructure.AppInfo;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Services.Monitoring;

namespace Smlp.Infrastructure.Monitoring;

/// <summary>
/// Wiring OpenTelemetry untuk WebApi. Mendukung dua exporter sekaligus (independen,
/// bisa dinyalakan/dimatikan lewat config tanpa saling mengganggu):
///   - Prometheus: expose endpoint /metrics untuk di-scrape (lalu divisualisasikan di Grafana).
///   - Application Insights: kirim metrics + traces langsung ke Azure Monitor.
/// Metrik bisnis (SmlpMetrics, didefinisikan di 03.Shared) ikut di-collect lewat AddMeter().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddMonitoringService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MonitoringOptions>(configuration.GetSection(MonitoringOptions.SectionKey));

        var monitoringOptions = configuration.GetSection(MonitoringOptions.SectionKey).Get<MonitoringOptions>();
        var appInfoOptions = configuration.GetSection(AppInfoOptions.SectionKey).Get<AppInfoOptions>();

        // Monitoring dimatikan total kalau kedua exporter Disabled — hemat overhead di lingkungan dev/lokal.
        if (monitoringOptions is null || (!monitoringOptions.Prometheus.Enabled && !monitoringOptions.ApplicationInsights.Enabled))
            return services;

        var serviceName = !string.IsNullOrWhiteSpace(monitoringOptions.ServiceName)
            ? monitoringOptions.ServiceName
            : appInfoOptions?.FullName ?? CommonValueFor.EntryAssemblySimpleName ?? "SMLP";

        var otel = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: serviceName,
                serviceVersion: CommonValueFor.EntryAssemblyVersion?.ToString() ?? "1.0.0"));

        otel.WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(SmlpMetrics.MeterName);

            if (monitoringOptions.Prometheus.Enabled)
                metrics.AddPrometheusExporter();

            if (monitoringOptions.ApplicationInsights.Enabled)
                metrics.AddAzureMonitorMetricExporter(o => o.ConnectionString = monitoringOptions.ApplicationInsights.ConnectionString);
        });

        // Tracing hanya relevan kalau ada backend yang bisa menampilkannya (Prometheus tidak menyimpan trace).
        if (monitoringOptions.ApplicationInsights.Enabled)
        {
            otel.WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAzureMonitorTraceExporter(o => o.ConnectionString = monitoringOptions.ApplicationInsights.ConnectionString);
            });
        }

        return services;
    }

    /// <summary>
    /// Map endpoint scrape Prometheus. Dipanggil terpisah dari AddMonitoringService (bukan lewat
    /// UseInfrastructure/IApplicationBuilder) karena MapX butuh IEndpointRouteBuilder, yang baru
    /// tersedia setelah UseRouting() jalan. Panggil ini di Program.cs, sebaris dengan app.MapControllers().
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
