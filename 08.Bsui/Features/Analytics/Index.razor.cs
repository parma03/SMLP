using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Analytics.Queries.GetDashboardSummary;
using Smlp.Shared.Analytics.Queries.GetRevenueChart;

namespace Smlp.Bsui.Features.Analytics;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private string _selectedPeriod = "30d";
    private bool _isLoading = false;
    private DateTime? _customDateFrom = null;
    private DateTime? _customDateTo = null;

    private GetDashboardSummaryResponse? _summary;
    private GetRevenueChartResponse? _revenueChart;

    private string SelectedPeriodLabel => _selectedPeriod switch
    {
        "7d" => "7 hari terakhir",
        "30d" => "30 hari terakhir",
        "90d" => "3 bulan terakhir",
        "ytd" => "tahun ini",
        "custom" when _customDateFrom.HasValue && _customDateTo.HasValue =>
            $"{_customDateFrom.Value:dd MMM yyyy} — {_customDateTo.Value:dd MMM yyyy}",
        _ => "periode dipilih"
    };

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        SetupBreadcrumb();
        await LoadAnalyticsData();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active("Dashboard Analytics"),
        };
    }

    private async Task LoadAnalyticsData()
    {
        _isLoading = true;
        try
        {
            var (year, month) = GetPeriodYearMonth();

            var summaryTask = _analyticsService.GetDashboardSummaryAsync();
            var chartTask = _analyticsService.GetRevenueChartAsync(new GetRevenueChartRequest
            {
                Year = year,
                Month = month
            });

            await Task.WhenAll(summaryTask, chartTask);

            var summaryResult = await summaryTask;
            if (summaryResult.Result is not null)
                _summary = summaryResult.Result;
            else if (summaryResult.Error is not null)
                _snackbar.AddError($"Summary: {summaryResult.Error.ToDetail()}");

            var chartResult = await chartTask;
            if (chartResult.Result is not null)
                _revenueChart = chartResult.Result;
            else if (chartResult.Error is not null)
                _snackbar.AddError($"Chart: {chartResult.Error.ToDetail()}");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat analytics: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OnPeriodChanged(string period)
    {
        _selectedPeriod = period;
        if (period != "custom")
        {
            _customDateFrom = null;
            _customDateTo = null;
            await LoadAnalyticsData();
        }
    }

    private (int year, int month) GetPeriodYearMonth()
    {
        if (_selectedPeriod == "custom" && _customDateFrom.HasValue)
        {
            return (_customDateFrom.Value.Year, _customDateFrom.Value.Month);
        }
        return _selectedPeriod switch
        {
            "7d" => (DateTime.Today.Year, DateTime.Today.Month),
            "30d" => (DateTime.Today.Year, DateTime.Today.Month),
            "90d" => (DateTime.Today.Year, 0),
            "ytd" => (DateTime.Today.Year, 0),
            _ => (DateTime.Today.Year, DateTime.Today.Month)
        };
    }
}
