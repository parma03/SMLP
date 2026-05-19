using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Scholarships.Constants;

namespace Smlp.Bsui.Features.UnivRegistration;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsShowUniversityRecommendation { get; set; } = true;
    public bool IsShowUniversityAdmission { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity.IsAuthenticated)
        {

        }
    }

    private int _activeTabIndex = 0;
    private static readonly List<BreadcrumbItem> _breadcrumbItems1 = new();

    private void SetupBreadcrumb()
    {
        // Tambahkan breadcrumb sesuai tab yang aktif
        switch (_activeTabIndex)
        {
            case 0:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UnivRegistration));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityRecommendation));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UnivRegistration));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmission));
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
