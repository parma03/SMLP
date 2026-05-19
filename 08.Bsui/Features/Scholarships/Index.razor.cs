using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Shared.Scholarships.Constants;

namespace Psmp.Bsui.Features.Scholarships;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsShowScholarshipPlan { get; set; } = false;
    public bool IsShowScholarshipPosting { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity!.IsAuthenticated)
        {
            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains("scholarship.plan.view")).ToList().Count > 0)
            {
                IsShowScholarshipPlan = true;
            }

            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains("scholarship.posting.view")).ToList().Count > 0)
            {
                IsShowScholarshipPosting = true;
            }
        }
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.ScholarshipPlan),
            CommonBreadcrumbFor.Active(DisplayTextFor.ScholarshipPlan)
        };
    }
}
