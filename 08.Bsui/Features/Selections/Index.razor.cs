using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Selections.Constants;

namespace Smlp.Bsui.Features.Selections;

public partial class Index
{
    [Parameter]
    public string Category { get; set; } = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();

    protected override void OnInitialized()
    {
        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        if (Category == "Close")
        {
            _breadcrumbItems = new()
            {
                CommonBreadcrumbFor.Active(DisplayTextFor.CompanySelection),
                CommonBreadcrumbFor.Active(DisplayTextFor.CloseRegistration),
            };
        }
        else
        {
            _breadcrumbItems = new()
            {
                CommonBreadcrumbFor.Active(DisplayTextFor.CompanySelection),
                CommonBreadcrumbFor.Active(DisplayTextFor.OpenRegistration),
            };
        }

    }
}
