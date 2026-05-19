using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Services.Authentication.Constants;

namespace Psmp.Bsui.Common.Pages;

public partial class MySession
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private MudMessageBox? _messageBoxAccessToken;
    private bool _inProductionEnvironment;
    private string _accessToken = default!;

    protected override void OnInitialized()
    {
        SetupBreadcrumb();

        _inProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active(AuthenticationDisplayTextFor.MySession)
        };
    }

    private async void ShowMessageBoxAccessToken()
    {
        await _messageBoxAccessToken!.Show();
    }
}
