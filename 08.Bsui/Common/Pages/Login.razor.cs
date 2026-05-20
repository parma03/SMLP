using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Smlp.Bsui.Common.Pages;

public partial class Login
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private bool _processing = false;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string? _errorMessage = null;
    private InputType _passwordInputType = InputType.Password;

    private async Task OnClickLoginAsync()
    {
        _processing = true;
        _errorMessage = null;
        await Task.Delay(500);
        NavigationManager.NavigateTo(Smlp.Bsui.Pages.Account.Constants.RouteFor.Login, true);
        _processing = false;
    }
}
