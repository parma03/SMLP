using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Analytics.Queries.GetCustomerInsights;
using System.Security.Claims;

namespace Smlp.Bsui.Features.CustomerInsights;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;

    private GetCustomerInsightsResponse? _insights;

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
        try
        {
            var result = await _analyticsService.GetCustomerInsightsAsync();
            if (result.Result is not null)
                _insights = result.Result;
            else if (result.Error is not null)
                _snackbar.AddError(result.Error.ToDetail());
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat customer insights: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private double RetentionRate => (double)(_insights?.RetentionRate ?? 0);
    private int TotalSegmented => (_insights?.NewCount ?? 0) + (_insights?.ActiveCount ?? 0) +
                                  (_insights?.LoyalCount ?? 0) + (_insights?.AtRiskCount ?? 0) +
                                  (_insights?.InactiveCount ?? 0);
}
