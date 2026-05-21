using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Analytics.Queries.GetDashboardSummary;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.Bsui.Features.Dashboards;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;

    private GetDashboardSummaryResponse? _summary;
    private List<GetTransactionResponse> _recentTransactions = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        SetupBreadcrumb();
        await LoadDashboardAsync();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active("Dashboard"),
        };
    }

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

    private decimal RevenueGrowth => _summary?.RevenueGrowthPercent ?? 0;
    private string RevenueGrowthLabel => RevenueGrowth >= 0
        ? $"+{RevenueGrowth:F1}% vs bulan lalu"
        : $"{RevenueGrowth:F1}% vs bulan lalu";
    private Color RevenueGrowthColor => RevenueGrowth >= 0 ? Color.Success : Color.Error;
}
