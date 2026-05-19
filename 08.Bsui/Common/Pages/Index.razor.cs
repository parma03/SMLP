using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;

namespace Smlp.Bsui.Common.Pages;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private string _greetings = default!;

    protected override void OnInitialized()
    {
        SetupBreadcrumb();

        _greetings = $"Good {DateTimeOffset.Now.ToFriendlyTimeDisplayText()}";
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(CommonDisplayTextFor.Home)
        };
    }
}
