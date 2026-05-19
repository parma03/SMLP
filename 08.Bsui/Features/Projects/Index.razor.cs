using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Shared.Scholarships.Constants;

namespace Psmp.Bsui.Features.Projects;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsShowProjectProposal { get; set; } = true;
    public bool IsShowProjectMentoring { get; set; } = true;
    public bool IsShowProjectCharterPlan { get; set; } = true;
    public bool IsShowProjectCharterReal { get; set; } = true;

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
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.InnovationProject));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ProjectProposal));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.InnovationProject));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ProjectMentoring));
                break;
            case 2:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.InnovationProject));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ProjectCharterPlan));
                break;
            case 3:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.InnovationProject));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ProjectCharterReal));
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
