using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Smlp.Bsui.Services.Authentication.Constants;
using Smlp.Bsui.Services.Authentication.Extensions;

namespace Smlp.Bsui.Services.Authentication.IS4IM;

public static class DependencyInjection
{
    public static IServiceCollection AddIS4IMAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IS4IMAuthenticationOptions>(configuration.GetSection(IS4IMAuthenticationOptions.SectionKey));

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var is4imAuthenticationOptions = configuration.GetSection(IS4IMAuthenticationOptions.SectionKey).Get<IS4IMAuthenticationOptions>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = is4imAuthenticationOptions.AuthorityUrl;
                options.ClientId = is4imAuthenticationOptions.ClientId;
                options.ClientSecret = is4imAuthenticationOptions.ClientSecret;
                options.ResponseType = OidcConstants.ResponseTypes.CodeIdToken;
                options.SaveTokens = true;
                options.UseTokenLifetime = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Add(IS4IMScopes.ApiAuth);
                options.Scope.Add(OidcConstants.StandardScopes.OfflineAccess);
                options.Scope.Add(is4imAuthenticationOptions.ApiAudienceScope);

                options.ClaimActions.MapJsonKey(ClaimTypes.Name, JwtClaimTypes.Email, ClaimValueTypes.String);
                options.ClaimActions.MapUniqueJsonKey(CustomClaimTypes.CompanyCode, CustomClaimTypes.CompanyCode, ClaimValueTypes.String);
                options.ClaimActions.MapUniqueJsonKey(CustomClaimTypes.EmployeeId, CustomClaimTypes.EmployeeId, ClaimValueTypes.String);
                options.ClaimActions.MapUniqueJsonKey(CustomClaimTypes.DisplayName, CustomClaimTypes.DisplayName, ClaimValueTypes.String);

                options.Events = new OpenIdConnectEvents
                {
                    OnTokenResponseReceived = async context => await context.SaveTokens(),
                    OnUserInformationReceived = async context => await context.LoadAuthorizationClaims()
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        return services;
    }

    public static IApplicationBuilder UseIS4IMAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();

        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        app.UseForwardedHeaders(forwardedHeadersOptions);

        return app;
    }
}
