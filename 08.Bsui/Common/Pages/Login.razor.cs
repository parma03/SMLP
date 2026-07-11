using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Services.Authentication;
using Smlp.Client.Services.BackEnd.Auth;
using Smlp.Shared.Auth.Requests;

namespace Smlp.Bsui.Common.Pages;

public partial class Login : IDisposable
{
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public AuthorizedAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] public AuthService AuthService { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    private bool _processing = false;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private InputType _passwordInputType = InputType.Password;
    private bool _showPassword = false;

    protected override async Task OnInitializedAsync()
    {
        // Guard: kalau user sudah authenticated (mis. kembali/refresh di URL /Login
        // padahal token masih valid), redirect ke dashboard alih-alih menampilkan
        // form login di dalam shell yang sudah authorized.
        AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        RedirectIfAuthenticated(authState);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _ = HandleAuthenticationStateChangedAsync(task);
    }

    private async Task HandleAuthenticationStateChangedAsync(Task<AuthenticationState> task)
    {
        var authState = await task;
        RedirectIfAuthenticated(authState);
    }

    private void RedirectIfAuthenticated(AuthenticationState authState)
    {
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            NavigationManager.NavigateTo("/", true);
        }
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
        _passwordInputType = _showPassword ? InputType.Text : InputType.Password;
    }

    private async Task OnClickLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
        {
            Snackbar.AddError("Username dan password wajib diisi.");
            return;
        }

        _processing = true;

        try
        {
            var request = new LoginRequest { Username = _username, Password = _password };
            var result = await AuthService.LoginAsync(request);

            if (result.Error is not null)
            {
                Snackbar.AddError(result.Error.Detail ?? "Login gagal. Periksa username dan password Anda.");
                return;
            }

            if (result.Result is not null)
            {
                await AuthStateProvider.LoginAsync(result.Result.AccessToken, result.Result.RefreshToken);
                NavigationManager.NavigateTo("/", true);
            }
        }
        catch (Exception)
        {
            Snackbar.AddError("Terjadi kesalahan. Silakan coba lagi.");
        }
        finally
        {
            _processing = false;
        }
    }

    private async Task OnKeyPressAsync(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await OnClickLoginAsync();
    }
}
