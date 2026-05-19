using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Shared.Scholarships.Constants;

namespace Psmp.Bsui.Features.Universities;

public partial class Index
{
    [Parameter]
    public int TabIndex { get; set; }

    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsShowUniversityRecommendation { get; set; } = true;
    public bool IsShowUniversityAdmission { get; set; } = true;

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
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityRegistration));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityRecommendation));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityRegistration));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmission));
                break;
        }

        //StateHasChanged();
    }
    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb(); // Perbarui breadcrumb setiap kali tab berubah
    }
}
