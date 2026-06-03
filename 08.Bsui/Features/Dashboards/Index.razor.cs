using System.Security.Claims;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Analytics.Queries.GetDashboardSummary;
using Smlp.Shared.Transaction.Transactions.Queries;
using Timer = System.Timers.Timer;

namespace Smlp.Bsui.Features.Dashboards;

public partial class Index : IDisposable
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;
    private bool _isPolling = false;

    private GetDashboardSummaryResponse? _summary;
    private List<GetTransactionResponse> _recentTransactions = new();

    // Live metrics polling
    private Timer? _pollingTimer;
    private DateTimeOffset _lastRefreshed = DateTimeOffset.Now;
    private int _pollingIntervalSeconds = 30;
    private bool _liveMetricsEnabled = true;
    private int _pollCountdown = 30;
    private Timer? _countdownTimer;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        SetupBreadcrumb();
        await LoadDashboardAsync();
        StartLiveMetrics();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active("Dashboard"),
        };
    }

    // ── Live Metrics ──────────────────────────────────────────────────────────

    private void StartLiveMetrics()
    {
        _pollingTimer = new Timer(TimeSpan.FromSeconds(_pollingIntervalSeconds).TotalMilliseconds);
        _pollingTimer.Elapsed += async (s, e) => await OnPollingTick(s, e);
        _pollingTimer.AutoReset = true;
        _pollingTimer.Start();

        _pollCountdown = _pollingIntervalSeconds;
        _countdownTimer = new Timer(1000);
        _countdownTimer.Elapsed += OnCountdownTick;
        _countdownTimer.AutoReset = true;
        _countdownTimer.Start();
    }

    private void OnCountdownTick(object? sender, ElapsedEventArgs e)
    {
        _pollCountdown--;
        if (_pollCountdown < 0) _pollCountdown = _pollingIntervalSeconds;
        InvokeAsync(StateHasChanged);
    }

    private async Task OnPollingTick(object? sender, ElapsedEventArgs e)
    {
        if (!_liveMetricsEnabled || _isPolling) return;
        _isPolling = true;
        try
        {
            var result = await _analyticsService.GetDashboardSummaryAsync();
            if (result.Result is not null)
            {
                _summary = result.Result;
                _lastRefreshed = DateTimeOffset.Now;
                _pollCountdown = _pollingIntervalSeconds;
            }
        }
        catch
        {
            // Silent fail for background polling
        }
        finally
        {
            _isPolling = false;
        }
        await InvokeAsync(StateHasChanged);
    }

    private void ToggleLiveMetrics()
    {
        _liveMetricsEnabled = !_liveMetricsEnabled;
        if (_liveMetricsEnabled)
        {
            _pollingTimer?.Start();
            _countdownTimer?.Start();
        }
        else
        {
            _pollingTimer?.Stop();
            _countdownTimer?.Stop();
        }
    }

    private async Task ManualRefreshAsync()
    {
        _pollCountdown = _pollingIntervalSeconds;
        await LoadDashboardAsync();
        _lastRefreshed = DateTimeOffset.Now;
    }

    // ── Data Load ─────────────────────────────────────────────────────────────

    private async Task LoadDashboardAsync()
    {
        _isLoading = true;
        try
        {
            var summaryTask = _analyticsService.GetDashboardSummaryAsync();
            var recentTask = _transactionService.GetTransactionsAsync(new GetTransactionRequest
            {
                PageNumber = 1,
                PageSize = 8,
                DateFrom = DateTimeOffset.Now.AddDays(-7)
            });

            await Task.WhenAll(summaryTask, recentTask);

            var summaryResult = await summaryTask;
            if (summaryResult.Result is not null)
                _summary = summaryResult.Result;

            var recentResult = await recentTask;
            if (recentResult.Result is not null)
                _recentTransactions = recentResult.Result.Items.ToList();

            _lastRefreshed = DateTimeOffset.Now;
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat dashboard: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    // ── Computed ──────────────────────────────────────────────────────────────

    private decimal RevenueGrowth => _summary?.RevenueGrowthPercent ?? 0;
    private string RevenueGrowthLabel => RevenueGrowth >= 0
        ? $"+{RevenueGrowth:F1}% vs bulan lalu"
        : $"{RevenueGrowth:F1}% vs bulan lalu";
    private Color RevenueGrowthColor => RevenueGrowth >= 0 ? Color.Success : Color.Error;

    private string LastRefreshedLabel =>
        $"Diperbarui {_lastRefreshed.LocalDateTime.ToString("HH:mm:ss")}";

    private string PollCountdownLabel =>
        _liveMetricsEnabled ? $"Refresh otomatis dalam {_pollCountdown}s" : "Live metrics nonaktif";

    private Color LiveIndicatorColor =>
        _liveMetricsEnabled ? Color.Success : Color.Default;

    private string LiveIcon =>
        _liveMetricsEnabled ? Icons.Material.Filled.FiberManualRecord : Icons.Material.Filled.PauseCircle;

    // ── Dispose ───────────────────────────────────────────────────────────────

    public void Dispose()
    {
        _pollingTimer?.Stop();
        _pollingTimer?.Dispose();
        _countdownTimer?.Stop();
        _countdownTimer?.Dispose();
    }
}
