using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.SiteAdministrator.Constants;

namespace Smlp.Bsui.Features.SiteAdministrator;

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
