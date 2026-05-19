using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Pages.Account.Constants;
using Smlp.Bsui.Services.Authorization.Components;
using Smlp.Shared.Services.Authentication.Constants;

namespace Smlp.Bsui.Services.Authentication.Components;

public partial class AccountInfo
{
    private void NavigateToProfile()
    {
        _navigationManager.NavigateTo(CommonRouteFor.Profile, true);
    }

    private void NavigateToMySession()
    {
        _navigationManager.NavigateTo(CommonRouteFor.MySession, true);
    }

    private async Task ShowDialogSwitchPosition()
    {
        var dialog = _dialogService.Show<DialogSwitchPosition>(AuthenticationDisplayTextFor.SwitchPosition);
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            StateHasChanged();
        }
    }

    private void NavigateToLogout()
    {
        _navigationManager.NavigateTo(RouteFor.Logout, true);
    }
}
