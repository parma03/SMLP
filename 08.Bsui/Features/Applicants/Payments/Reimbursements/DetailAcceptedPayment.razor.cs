using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Payments.Contants;

namespace Smlp.Bsui.Features.Applicants.Payments.Reimbursements;
public partial class DetailAcceptedPayment
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }
    [Parameter]
    public Guid ApplicantId { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Private Properties

    private int _activeTabIndex = 0;
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    #endregion
    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
    }
    private void SetupBreadcrumb()
    {
        // Tambahkan breadcrumb sesuai tab yang aktif
        switch (_activeTabIndex)
        {
            case 0:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Payment));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.DraftingBA));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Payment));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.FinalBA));
                break;
        }
    }
    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb(); // Perbarui breadcrumb setiap kali tab berubah
    }
}
