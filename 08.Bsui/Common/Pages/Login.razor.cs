/*
*/

using Microsoft.AspNetCore.Components;

namespace Smlp.Bsui.Common.Pages;

public partial class Login
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private bool _processing = false;
    private const int DelayTime = 2000;

    private async Task OnClickLoginAsync()
    {
        _processing = true;
        NavigationManager.NavigateTo(Smlp.Bsui.Pages.Account.Constants.RouteFor.Login, true);
        await Task.Delay(DelayTime);
        _processing = false;
    }
}
