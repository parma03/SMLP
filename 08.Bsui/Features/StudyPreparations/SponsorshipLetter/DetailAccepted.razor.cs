using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Base.Studies.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.StudyPreparations.SponsorshipLetter.Constants;

namespace Psmp.Bsui.Features.StudyPreparations.SponsorshipLetter;

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
        var dataResponse = await _studyPreparationSponsorshipLetterService.GetStudyPreparationSponsorshipLetterByIdAsync(Id);
        if (dataResponse.Result == null || dataResponse.Result.Id == Guid.Empty)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(4), true);
            StateHasChanged();
            return;
        }

        var data = dataResponse.Result;
        var canAccess = true;

        switch (data.Status)
        {
            case SponsorshipLetterStatus.Accepted:
            case SponsorshipLetterStatus.ProcessSign:
            case SponsorshipLetterStatus.ProcessToPOffice:
            case SponsorshipLetterStatus.Draft:
            case SponsorshipLetterStatus.Completed:
            case SponsorshipLetterStatus.Rejected:
                canAccess = true;
                break;
            case SponsorshipLetterStatus.WaitingApproval:
                canAccess = false;
                break;
        }

        if (!canAccess)
        {
            _snackbar.AddError("Can not access this data!!");
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(4), true);
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
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.SponsorshipLetter));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.DraftingSponsorshipLetter));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparations));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.SponsorshipLetter));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.FinalSponsorshipLetter));
                break;
        }
    }
    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb(); // Perbarui breadcrumb setiap kali tab berubah
    }
}
