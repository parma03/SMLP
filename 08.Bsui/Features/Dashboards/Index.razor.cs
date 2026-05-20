using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;

namespace Smlp.Bsui.Features.Dashboards;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    private List<RecentTransaction> _recentTransactions = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active("Dashboard"),
        };
    }

    public record RecentTransaction(string CustomerName, string Time, decimal Amount, int Points);
}
