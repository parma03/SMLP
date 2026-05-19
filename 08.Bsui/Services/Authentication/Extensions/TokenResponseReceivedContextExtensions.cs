using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Smlp.Bsui.Services.Authentication.Extensions;

public static class TokenResponseReceivedContextExtensions
{
    public static Task SaveTokens(this TokenResponseReceivedContext context)
    {
        var principal = context.Principal;

        if (principal is null)
        {
            return Task.CompletedTask;
        }

        if (principal.Identity is not ClaimsIdentity identity)
        {
            return Task.CompletedTask;
        }

        identity.AddClaim(new Claim(OidcConstants.TokenResponse.AccessToken, context.TokenEndpointResponse.AccessToken));
        identity.AddClaim(new Claim(OidcConstants.TokenResponse.RefreshToken, context.TokenEndpointResponse.RefreshToken));

        return Task.CompletedTask;
    }
}
