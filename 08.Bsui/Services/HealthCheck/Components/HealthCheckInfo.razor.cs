using MudBlazor;
using Psmp.Shared.Services.HealthCheck.Constants;
using Psmp.Shared.Services.HealthCheck.Queries.GetHealthCheck;

using Timer = System.Timers.Timer;

namespace Psmp.Bsui.Services.HealthCheck.Components;

public partial class HealthCheckInfo
{
    private Severity _healthCheckSeverity = Severity.Info;
    private string _healthCheckStatus = HealthCheckStatus.Loading;
    private Dictionary<string, GetHealthCheckHealthCheckEntry> _healthCheckEntries = new();

    protected override void OnInitialized()
    {
        var timerForHealthCheck = new Timer();
        timerForHealthCheck.Elapsed += async (s, e) => await GetHealthCheck();
        timerForHealthCheck.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
        timerForHealthCheck.Start();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetHealthCheck();

            StateHasChanged();
        }
    }

    private async Task GetHealthCheck()
    {
        var response = await _healthCheckService.GetHealthCheckAsync(_backEndOptions.Value.HealthCheckApiUrl);

        _healthCheckSeverity = response.Status switch
        {
            HealthCheckStatus.Loading => Severity.Info,
            HealthCheckStatus.Healthy => Severity.Success,
            HealthCheckStatus.Unhealthy => Severity.Error,
            HealthCheckStatus.Degraded => Severity.Warning,
            HealthCheckStatus.Unknown => Severity.Error,
            _ => Severity.Error
        };

        _healthCheckStatus = response.Status;
        _healthCheckEntries = new Dictionary<string, GetHealthCheckHealthCheckEntry>(response.Entries);
    }
}
