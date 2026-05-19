using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Shared.SiteAdministrator.Constants;

namespace Psmp.Bsui.Features.SiteAdministrator;

public partial class Index
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
            CommonBreadcrumbFor.Active(DisplayTextFor.SiteAdministrator),
            CommonBreadcrumbFor.Active(DisplayTextFor.Company),
        };
    }
}
