using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Shared.Scholarships.Constants;

namespace Psmp.Bsui.Features.StudyPreparations;

public partial class Index
{
    [Parameter]
    public int TabIndex { get; set; }
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsShowIELTS { get; set; } = true;
    public bool IsShowGMAT { get; set; } = true;
    public bool IsShowGRE { get; set; } = true;
    public bool IsShowEssay { get; set; } = true;
    public bool IsShowSponsorshipLetter { get; set; } = true;
    public bool IsShowVisaGuarantee { get; set; } = true;

    private int _activeTabIndex = 0;

    protected override async Task OnInitializedAsync()
    {
        _activeTabIndex = TabIndex;
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
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparation));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.IELTS));
                break;

            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparation));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.GMAT));
                break;

            case 2:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparation));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.GRE));
                break;

            case 3:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparation));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Essay));
                break;

            case 4:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparation));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.SponsorshipLetter));
                break;

            case 5:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparation));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.VisaGuarantee));
                break;
        }

        StateHasChanged();
    }

    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb(); // Perbarui breadcrumb setiap kali tab berubah
    }
}
