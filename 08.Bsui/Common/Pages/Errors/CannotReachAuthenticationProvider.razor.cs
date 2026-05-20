using Smlp.Bsui.Services.Authentication;

namespace Smlp.Bsui.Common.Pages.Errors;

public partial class CannotReachAuthenticationProvider
{
    private string _authenticationProviderUrl = string.Empty;

    protected override void OnInitialized()
    {
        _authenticationProviderUrl = "Server autentikasi tidak dapat dijangkau.";
    }
}
