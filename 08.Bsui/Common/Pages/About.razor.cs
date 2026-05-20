using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Common.Constants;

namespace Smlp.Bsui.Common.Pages;

public partial class About
{
    private List<BreadcrumbItem> _breadcrumbItems = new();

    protected override void OnInitialized()
    {
        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active(CommonDisplayTextFor.About)
        };
    }
}
