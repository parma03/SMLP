using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Services.Authorization.Constants;
using Smlp.Shared.Scholarships.Constants;
using Smlp.Bsui.Features.Scholarships.ScholarshipPlan.Constants;

namespace Smlp.Bsui.Features.Scholarships.ScholarshipPlan.Draft;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    protected ClaimsPrincipal _user = default!;
    public bool IsAllowScholarshipPlanCrud { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity!.IsAuthenticated)
        {
            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains(Permissions.Psmp_ScholarshipPlan_Crud)).ToList().Count > 0)
            {
                IsAllowScholarshipPlanCrud = true;
            }
        }
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.Scholarship),
            CommonBreadcrumbFor.Active(DisplayTextFor.ScholarshipPlan),
            CommonBreadcrumbFor.Active(DisplayTextFor.DraftScholarshipPlan)
        };
    }

    private void OnClickBack()
    {
        _navigationManager.NavigateTo(RouteFor.Scholarships, true);
    }
}
