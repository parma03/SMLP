/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.AspNetCore.Components;

namespace Psmp.Bsui.Common.Pages;

public partial class Login
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private bool _processing = false;
    private const int DelayTime = 2000;

    private async Task OnClickLoginAsync()
    {
        _processing = true;
        NavigationManager.NavigateTo(Psmp.Bsui.Pages.Account.Constants.RouteFor.Login, true);
        await Task.Delay(DelayTime);
        _processing = false;
    }
}
