using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Psmp.Bsui.Services.Authentication.Constants;
using Psmp.Bsui.Services.Authentication.Extensions;
using Psmp.Bsui.Services.FrontEnd;

namespace Psmp.Bsui.Services.Authentication.IdAMan;

public static class DependencyInjection
{
    public static IServiceCollection AddIdAManAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdAManAuthenticationOptions>(configuration.GetSection(IdAManAuthenticationOptions.SectionKey));

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var idAManAuthenticationOptions = configuration.GetSection(IdAManAuthenticationOptions.SectionKey).Get<IdAManAuthenticationOptions>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = idAManAuthenticationOptions!.AuthorityUrl;
                options.ClientId = idAManAuthenticationOptions.ClientId;
                options.ClientSecret = idAManAuthenticationOptions.ClientSecret;
                options.ResponseType = OidcConstants.ResponseTypes.CodeIdToken;
                options.SaveTokens = true;
                options.UseTokenLifetime = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Add(IdAManScopes.ApiAuth);
                options.Scope.Add(IdAManScopes.UserRole);
                options.Scope.Add(IdAManScopes.UserRead);
                options.Scope.Add(IdAManScopes.ApplicationRead);
                options.Scope.Add(OidcConstants.StandardScopes.OfflineAccess);
                options.Scope.Add(idAManAuthenticationOptions.ApiAudienceScope);

                options.ClaimActions.MapJsonKey(ClaimTypes.Name, JwtClaimTypes.Email, ClaimValueTypes.String);
                options.ClaimActions.MapUniqueJsonKey(CustomClaimTypes.CompanyCode, CustomClaimTypes.CompanyCode, ClaimValueTypes.String);
                options.ClaimActions.MapUniqueJsonKey(CustomClaimTypes.EmployeeId, CustomClaimTypes.EmployeeId, ClaimValueTypes.String);
                options.ClaimActions.MapUniqueJsonKey(CustomClaimTypes.DisplayName, CustomClaimTypes.DisplayName, ClaimValueTypes.String);

                options.Events = new OpenIdConnectEvents
                {
                    OnTokenResponseReceived = async context => await context.SaveTokens(),
                    OnUserInformationReceived = async context => await context.LoadAuthorizationClaims(),
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (idAManAuthenticationOptions.Redirect.Enabled)
                        {
                            var frontEndOptions = configuration.GetSection(FrontEndOptions.SectionKey).Get<FrontEndOptions>();
                            var httpRequest = context.HttpContext.Request;
                            var redirectUri = $"{httpRequest.Scheme}://{httpRequest.Host}{frontEndOptions!.BasePath}/signin-oidc";

                            context.ProtocolMessage.RedirectUri = idAManAuthenticationOptions.Redirect.Url;
                        }

                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        return services;
    }

    public static IApplicationBuilder UseIdAManAuthentication(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseAuthentication();

        var idAManAuthenticationOptions = configuration.GetSection(IdAManAuthenticationOptions.SectionKey).Get<IdAManAuthenticationOptions>();

        var proxyIpAddresses = new List<IPAddress>();

        if (idAManAuthenticationOptions!.Proxy.Enabled)
        {
            foreach (var host in idAManAuthenticationOptions.Proxy.Hosts)
            {
                proxyIpAddresses.Add(IPAddress.Parse(host));
            }

            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";

                return next();
            });
        }

        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        forwardedHeadersOptions.KnownProxies.Clear();

        foreach (var proxyIpAddress in proxyIpAddresses)
        {
            forwardedHeadersOptions.KnownProxies.Add(proxyIpAddress);
        }

        app.UseForwardedHeaders(forwardedHeadersOptions);

        return app;
    }
}
