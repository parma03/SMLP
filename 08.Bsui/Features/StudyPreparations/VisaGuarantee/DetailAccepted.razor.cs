using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Base.Studies.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.StudyPreparations.VisaGuarantee.Constants;

namespace Smlp.Bsui.Features.StudyPreparations.VisaGuarantee;

public partial class DetailAccepted
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }

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
        var dataResponse = await _studyPreparationVisaGuaranteeService.GetStudyPreparationVisaGuaranteeByIdAsync(Id);
        if (dataResponse.Result == null || dataResponse.Result.Id == Guid.Empty)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(5), true);
            StateHasChanged();
            return;
        }

        var data = dataResponse.Result;
        var canAccess = true;

        switch (data.Status)
        {
            case VisaGuaranteeStatus.Accepted:
            case VisaGuaranteeStatus.ProcessSign:
            case VisaGuaranteeStatus.ProcessToPOffice:
            case VisaGuaranteeStatus.Draft:
            case VisaGuaranteeStatus.Completed:
            case VisaGuaranteeStatus.Rejected:
                canAccess = true;
                break;
            case VisaGuaranteeStatus.WaitingApproval:
                canAccess = false;
                break;
        }

        if (!canAccess)
        {
            _snackbar.AddError("Can not access this data!!");
            _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(5), true);
            StateHasChanged();
            return;
        }

        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
    }

    private void SetupBreadcrumb()
    {
        // Tambahkan breadcrumb sesuai tab yang aktif
        switch (_activeTabIndex)
        {
            case 0:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparations));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.VisaGuarantee));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.DraftingVisaGuarantee));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparations));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.VisaGuarantee));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.FinalVisaGuarantee));
                break;
        }
    }
    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb(); // Perbarui breadcrumb setiap kali tab berubah
    }
}
