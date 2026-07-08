using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Analytics.Queries.ExportAnalytics;
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
    private bool _dataLoaded = false;
    private DateTime? _customDateFrom = null;
    private DateTime? _customDateTo = null;

    private GetDashboardSummaryResponse? _summary;
    private GetRevenueChartResponse? _revenueChart;

    private decimal TotalRevenue => _revenueChart?.MonthlyRevenue?.Sum(x => x.Amount) ?? 0;
    private int TotalTransactions => _revenueChart?.MonthlyRevenue?.Sum(x => x.Transactions) ?? 0;
    private decimal AverageTransactionValue => TotalTransactions > 0 ? TotalRevenue / TotalTransactions : 0;
    private int TotalPointsIssued => _revenueChart?.MonthlyRevenue?.Sum(x => x.PointsEarned) ?? 0;

    private List<RevenueDataPoint> ChartDataPoints =>
        (_revenueChart?.DailyRevenue?.Any() == true ? _revenueChart.DailyRevenue : _revenueChart?.MonthlyRevenue) ?? new();

    private List<PaymentMethodBreakdownDisplay> PaymentBreakdowns
    {
        get
        {
            var methods = _revenueChart?.PaymentMethods ?? new();
            var total = methods.Sum(m => m.Amount);
            return methods
                .Select(m => new PaymentMethodBreakdownDisplay
                {
                    PaymentMethod = m.Method,
                    TotalRevenue = m.Amount,
                    Percentage = total > 0 ? m.Amount / total * 100 : 0
                })
                .ToList();
        }
    }

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
            _dataLoaded = true;
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

    private bool _isExporting = false;

    private async Task ExportAnalyticsAsync(string type)
    {
        if (_isExporting)
            return;
        _isExporting = true;
        try
        {
            var (year, month) = GetPeriodYearMonth();
            var result = await _analyticsService.ExportAsync(new ExportAnalyticsRequest
            {
                Type = type,
                Year = year,
                Month = month
            });

            if (result.Error is not null)
            {
                _snackbar.AddError($"Gagal export: {result.Error.ToDetail()}");
                return;
            }

            var base64 = Convert.ToBase64String(result.Result!.Content);
            await _jsRuntime.InvokeVoidAsync("downloadFileFromBase64",
                result.Result.FileName,
                result.Result.ContentType,
                base64);

            _snackbar.AddSuccess($"Export '{type}' berhasil diunduh.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Error export: {ex.Message}");
        }
        finally
        {
            _isExporting = false;
        }
    }
}

public class PaymentMethodBreakdownDisplay
{
    public string PaymentMethod { get; set; } = default!;
    public decimal TotalRevenue { get; set; }
    public decimal Percentage { get; set; }
}
