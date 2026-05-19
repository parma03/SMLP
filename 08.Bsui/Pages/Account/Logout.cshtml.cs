using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Services.Authentication.IdAMan;
using Smlp.Bsui.Services.Authentication.IS4IM;
using Smlp.Client.Services.HealthCheck;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Services.Authentication.Constants;
using Smlp.Shared.Services.HealthCheck.Constants;
using AuthenticationOptions = Smlp.Bsui.Services.Authentication.AuthenticationOptions;

namespace Smlp.Bsui.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly HealthCheckService _healthCheckService;
    private readonly bool _usingAuthentication;
    private readonly string? _authenticationHealthCheckUrl;

    public LogoutModel(
        HealthCheckService healthCheckService,
        IOptions<AuthenticationOptions> authenticationOptions,
        IConfiguration configuration)
    {
        _healthCheckService = healthCheckService;
        _usingAuthentication = authenticationOptions.Value.Provider is not AuthenticationProvider.None;

        switch (authenticationOptions.Value.Provider)
        {
            case AuthenticationProvider.None:
                _authenticationHealthCheckUrl = null;
                break;

            case AuthenticationProvider.IdAMan:
                var idAManAuthenticationOptions = configuration.GetSection(IdAManAuthenticationOptions.SectionKey).Get<IdAManAuthenticationOptions>();
                _authenticationHealthCheckUrl = idAManAuthenticationOptions!.HealthCheckUrl;
                break;

            case AuthenticationProvider.IS4IM:
                var is4imAuthenticationOptions = configuration.GetSection(IS4IMAuthenticationOptions.SectionKey).Get<IS4IMAuthenticationOptions>();
                _authenticationHealthCheckUrl = is4imAuthenticationOptions!.HealthCheckUrl;
                break;

            default:
                throw new ArgumentException($"{CommonDisplayTextFor.Unsupported} {nameof(Services.Authentication)} {nameof(AuthenticationOptions.Provider)}: {authenticationOptions.Value.Provider}");
        }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_usingAuthentication)
        {
            Response.Redirect(CommonRouteFor.Index);
        }

        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}",
        };

        if (!string.IsNullOrWhiteSpace(_authenticationHealthCheckUrl))
        {
            var authenticationHealthCheckResponse = await _healthCheckService.GetHealthCheckAsync(_authenticationHealthCheckUrl);

            if (authenticationHealthCheckResponse.Status is not HealthCheckStatus.Healthy)
            {
                return SignOut(authenticationProperties, CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }

        return SignOut(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
