using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Psmp.Bsui.Services.Authorization;
using Psmp.Shared.Services.Authorization.Constants;

namespace Psmp.Bsui.Services.Authentication.Extensions;

public static class UserInformationReceivedContextExtensions
{
    public static async Task LoadAuthorizationClaims(this UserInformationReceivedContext context)
    {
        var principal = context.Principal;

        if (principal is null)
        {
            return;
        }

        if (principal.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var username = context.User.RootElement.GetString(JwtClaimTypes.Email);

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException($"User information payload does not contain required JSON element: {JwtClaimTypes.Email}.");
        }

        var accessToken = identity.FindFirst(OidcConstants.TokenResponse.AccessToken)!.Value;

        var serviceProvider = context.HttpContext.RequestServices;
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
        var getPositionsResponse = await authorizationService.GetPositionsAsync(username, accessToken);

        if (getPositionsResponse.Positions.Any())
        {
            var defaultPosition = getPositionsResponse.Positions.FirstOrDefault(x => x.PersonaType.Equals(Personas.Permanent, StringComparison.OrdinalIgnoreCase));

            if (defaultPosition is null)
            {
                defaultPosition = getPositionsResponse.Positions.FirstOrDefault(x => x.PersonaType.Equals(Personas.Temporary, StringComparison.OrdinalIgnoreCase));
            }

            if (defaultPosition is null)
            {
                defaultPosition = getPositionsResponse.Positions.FirstOrDefault(x => x.PersonaType.Equals(Personas.Functional, StringComparison.OrdinalIgnoreCase));
            }

            if (defaultPosition is null)
            {
                defaultPosition = getPositionsResponse.Positions.FirstOrDefault(x => x.PersonaType.Equals(Personas.AdHoc, StringComparison.OrdinalIgnoreCase));
            }

            if (defaultPosition is not null)
            {
                if (!identity.Claims.Any(x => x.Type == AuthorizationClaimTypes.PositionId && x.Value == defaultPosition.Id))
                {
                    identity.AddClaim(new Claim(AuthorizationClaimTypes.PositionId, defaultPosition.Id, ClaimValueTypes.String));
                }

                if (!identity.Claims.Any(x => x.Type == AuthorizationClaimTypes.PositionName && x.Value == defaultPosition.Name))
                {
                    identity.AddClaim(new Claim(AuthorizationClaimTypes.PositionName, defaultPosition.Name, ClaimValueTypes.String));
                }

                var authorizationInfo = await authorizationService.GetAuthorizationInfoAsync(defaultPosition.Id, accessToken);

                foreach (var permission in authorizationInfo.Roles.SelectMany(x => x.Permissions))
                {
                    if (!identity.Claims.Any(x => x.Type == AuthorizationClaimTypes.Permission && x.Value == permission))
                    {
                        identity.AddClaim(new Claim(AuthorizationClaimTypes.Permission, permission, ClaimValueTypes.String));
                    }
                }

                foreach (var customParameter in authorizationInfo.CustomParameters)
                {
                    var customParameterClaimType = $"{AuthorizationClaimTypes.CustomParameter}{AuthorizationDelimiterFor.CustomParameter}{customParameter.Key}";

                    if (!identity.Claims.Any(x => x.Type == customParameterClaimType && x.Value == customParameter.Value))
                    {
                        identity.AddClaim(new Claim(customParameterClaimType, customParameter.Value, ClaimValueTypes.String));
                    }
                }
            }
        }
    }
}
