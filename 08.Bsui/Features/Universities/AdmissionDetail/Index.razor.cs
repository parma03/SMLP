using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Scholarships.Constants;

namespace Smlp.Bsui.Features.Universities.AdmissionDetail;

public partial class Index
{
    [Parameter]
    public Guid Id { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;

    private int _activeTabIndex = 0;
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();

    protected override async Task OnInitializedAsync()
    {
        // validation if not exist admission
        var dataAdmission = await _universityAdmissionService.GetUniversityAdmissionByIdAsync(Id);
        if (dataAdmission.Result == null || dataAdmission.Result.Id == Guid.Empty)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Smlp.Bsui.Features.Universities.Constants.RouteFor.UniversityAdmissionTab(1), true);
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
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityRegistration));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmissionDetail));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmissionLoASuperiorLetter));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityRegistration));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmissionDetail));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmissionDraftWarrant));
                break;
            case 2:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityRegistration));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmissionDetail));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmissionDraftAgreement));
                break;
            case 3:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityRegistration));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmissionDetail));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmissionFinalLetter));
                break;
        }
    }
    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb(); // Perbarui breadcrumb setiap kali tab berubah
    }
}
