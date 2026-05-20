/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using System.Security.Claims;

namespace Smlp.Bsui.Features.Analytics;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private DateRange _dateRange = new(DateTime.Today.AddDays(-30), DateTime.Today);
    private string _selectedPeriod = "30d";
    private bool _isLoading = false;

    // KPI Summary
    private int _totalRevenue = 0;
    private int _totalTransactions = 0;
    private int _activeMembers = 0;
    private decimal _avgTransactionValue = 0;

    // Revenue Trend (placeholder data for chart)
    private List<RevenuePoint> _revenueTrend = new();

    // Top Products
    private List<TopProduct> _topProducts = new();

    // Peak Hours
    private List<PeakHour> _peakHours = new();

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

        // TODO: Load from API services
        // var result = await _analyticsService.GetDashboardAnalyticsAsync(_dateRange.Start, _dateRange.End);
        // Placeholder data for UI skeleton
        _revenueTrend = GeneratePlaceholderRevenueTrend();
        _topProducts = GeneratePlaceholderTopProducts();
        _peakHours = GeneratePlaceholderPeakHours();

        _isLoading = false;
    }

    private async Task OnPeriodChanged(string period)
    {
        _selectedPeriod = period;
        _dateRange = period switch
        {
            "7d" => new DateRange(DateTime.Today.AddDays(-7), DateTime.Today),
            "30d" => new DateRange(DateTime.Today.AddDays(-30), DateTime.Today),
            "90d" => new DateRange(DateTime.Today.AddDays(-90), DateTime.Today),
            "ytd" => new DateRange(new DateTime(DateTime.Today.Year, 1, 1), DateTime.Today),
            _ => _dateRange
        };
        await LoadAnalyticsData();
    }

    private List<RevenuePoint> GeneratePlaceholderRevenueTrend()
    {
        var result = new List<RevenuePoint>();
        var days = (int)(_dateRange.End!.Value - _dateRange.Start!.Value).TotalDays;
        for (int i = 0; i <= Math.Min(days, 30); i++)
        {
            result.Add(new RevenuePoint(_dateRange.Start.Value.AddDays(i).ToString("dd/MM"), 0));
        }
        return result;
    }

    private List<TopProduct> GeneratePlaceholderTopProducts() => new()
    {
        new("Americano", 0, 0),
        new("Latte", 0, 0),
        new("Cappuccino", 0, 0),
        new("Croissant", 0, 0),
        new("Es Kopi Susu", 0, 0),
    };

    private List<PeakHour> GeneratePlaceholderPeakHours() => new()
    {
        new("07:00-09:00", 0), new("09:00-11:00", 0), new("11:00-13:00", 0),
        new("13:00-15:00", 0), new("15:00-17:00", 0), new("17:00-19:00", 0),
    };

    public record RevenuePoint(string Date, decimal Revenue);
    public record TopProduct(string Name, int Qty, decimal Revenue);
    public record PeakHour(string TimeSlot, int Transactions);
}
