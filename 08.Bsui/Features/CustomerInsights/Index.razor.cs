/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using System.Security.Claims;

namespace Smlp.Bsui.Features.CustomerInsights;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;
    private string _activeTab = "rfm";

    // RFM Segments
    private List<SegmentRow> _segments = new()
    {
        new("🌟 Champions", "Beli baru-baru ini, sering, dan banyak", Color.Success, "Hadiahi dan minta review", 0, 0),
        new("💎 Loyal Customers", "Beli sering & banyak tapi baru mulai", Color.Info, "Tawarkan program loyalitas premium", 0, 0),
        new("🚀 Potential Loyalists", "Member baru dengan potensi tinggi", Color.Primary, "Onboarding rewards & edukasi program", 0, 0),
        new("⚠️ At Risk", "Pernah aktif, kini mulai jarang", Color.Warning, "Kirim penawaran comeback dan ingatkan poin", 0, 0),
        new("😴 Hibernating", "Tidak beli dalam 60+ hari", Color.Error, "Kampanye win-back dengan diskon spesial", 0, 0),
        new("👋 New Customers", "Baru bergabung, 1-2 transaksi", Color.Secondary, "Welcome series dan first reward", 0, 0),
    };

    // Cohort Analysis (placeholder)
    private List<CohortRow> _cohortData = new()
    {
        new("Jan 2025", 0, 0, 0, 0),
        new("Feb 2025", 0, 0, 0, 0),
        new("Mar 2025", 0, 0, 0, 0),
    };

    // Churn Risk
    private List<ChurnRiskRow> _churnRisk = new();

    // Birthday upcoming
    private List<BirthdayRow> _upcomingBirthdays = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        SetupBreadcrumb();
        await LoadData();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active("Customer Insights"),
        };
    }

    private async Task LoadData()
    {
        _isLoading = true;
        // TODO: Load RFM, cohort, churn data from services
        await Task.Delay(1); // placeholder
        _isLoading = false;
    }

    public record SegmentRow(string Name, string Description, Color Color, string Action, int Count, decimal Percentage);
    public record CohortRow(string Month, int NewCustomers, int RetentionMonth1, int RetentionMonth2, int RetentionMonth3);
    public record ChurnRiskRow(string CustomerName, string MemberCode, int DaysSinceLastVisit, int TotalPoints, string Tier);
    public record BirthdayRow(string CustomerName, string PhoneNumber, DateOnly BirthDate, string Tier, int DaysUntilBirthday);
}
