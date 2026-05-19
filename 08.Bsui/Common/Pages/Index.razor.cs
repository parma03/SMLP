using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;

namespace Psmp.Bsui.Common.Pages;

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
