using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Bsui.Common.Constants;

namespace Smlp.Bsui.Services.Authentication.Components;

public partial class AccountInfo
{
    [Inject] private NavigationManager _navigationManager { get; set; } = default!;
    [Inject] private IDialogService _dialogService { get; set; } = default!;
    [Inject] private AuthorizedAuthenticationStateProvider _authStateProvider { get; set; } = default!;

    private void NavigateToProfile()
    {
        _navigationManager.NavigateTo(CommonRouteFor.Profile, true);
    }

    private async Task LogoutAsync()
    {
        await _authStateProvider.LogoutAsync();
        _navigationManager.NavigateTo("/Login", true);
    }
}
