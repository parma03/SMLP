using Smlp.Bsui.Services.Authorization;

namespace Smlp.Bsui.Common.Pages.Errors;

public partial class CannotReachAuthorizationProvider
{
    private string _authorizationProviderUrl = string.Empty;

    protected override void OnInitialized()
    {
        _authorizationProviderUrl = "Server otorisasi tidak dapat dijangkau.";
    }
}
