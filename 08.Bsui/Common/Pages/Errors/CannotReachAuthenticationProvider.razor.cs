using Smlp.Bsui.Services.Authentication.IdAMan;
using Smlp.Shared.Services.Authentication.Constants;

namespace Smlp.Bsui.Common.Pages.Errors;

public partial class CannotReachAuthenticationProvider
{
    private string _authenticationProviderUrl = default!;

    protected override void OnInitialized()
    {
        switch (_authenticationOptions.Value.Provider)
        {
            case AuthenticationProvider.None:
                break;

            case AuthenticationProvider.IdAMan:
                var idAManAuthenticationOptions = configuration.GetSection(IdAManAuthenticationOptions.SectionKey).Get<IdAManAuthenticationOptions>();
                _authenticationProviderUrl = idAManAuthenticationOptions.AuthorityUrl;
                break;

            default:
                break;
        }
    }
}
