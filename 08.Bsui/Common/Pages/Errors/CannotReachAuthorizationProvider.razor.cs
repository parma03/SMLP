using Smlp.Bsui.Services.Authorization.IdAMan;
using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.Bsui.Common.Pages.Errors;

public partial class CannotReachAuthorizationProvider
{
    private string _authorizationProviderUrl = default!;

    protected override void OnInitialized()
    {
        switch (_authorizationOptions.Value.Provider)
        {
            case AuthorizationProvider.None:
                break;

            case AuthorizationProvider.IdAMan:
                var idAManAuthorizationOptions = configuration.GetSection(IdAManAuthorizationOptions.SectionKey).Get<IdAManAuthorizationOptions>();
                _authorizationProviderUrl = idAManAuthorizationOptions.BaseUrl;
                break;

            default:
                break;
        }
    }
}
