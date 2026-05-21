using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;

namespace Smlp.Bsui.Features.LoyaltyAnalysis;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;
    private GetLoyaltyAnalysisResponse? _analysis;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        await LoadAnalysisAsync();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active("Loyalty Analysis"),
        };
    }

    private async Task LoadAnalysisAsync()
    {
        _isLoading = true;
        try
        {
            var result = await _analyticsService.GetLoyaltyAnalysisAsync();
            if (result.Result is not null)
                _analysis = result.Result;
            else if (result.Error is not null)
                _snackbar.AddError(result.Error.ToDetail());
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat loyalty analysis: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private double RedemptionRate => (double)(_analysis?.RedemptionRate ?? 0);
    private long TotalIssued => _analysis?.TotalPointsIssued ?? 0;
    private long TotalRedeemed => _analysis?.TotalPointsRedeemed ?? 0;
    private long TotalCirculating => _analysis?.TotalPointsCirculating ?? 0;
}
