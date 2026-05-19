using Psmp.Bsui.Services.Authorization.IdAMan;
using Psmp.Bsui.Services.Authorization.IS4IM;
using Psmp.Bsui.Services.Authorization.None;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Services.Authentication.Constants;
using Psmp.Shared.Services.Authorization.Constants;

namespace Psmp.Bsui.Services.Authorization;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorizationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthorizationOptions>(configuration.GetSection(AuthorizationOptions.SectionKey));
        var authorizationOptions = configuration.GetSection(AuthorizationOptions.SectionKey).Get<AuthorizationOptions>();

        switch (authorizationOptions!.Provider)
        {
            case AuthorizationProvider.None:
                services.AddNoneAuthorizationService();
                break;

            case AuthorizationProvider.IdAMan:
                services.AddIdAManAuthorizationService(configuration);
                break;

            case AuthorizationProvider.IS4IM:
                services.AddIS4IMAuthorizationService(configuration);
                break;

            default:
                throw new ArgumentException($"{CommonDisplayTextFor.Unsupported} {nameof(Authorization)} {nameof(AuthorizationOptions.Provider)}: {authorizationOptions.Provider}");
        }

        if (authorizationOptions.Provider != AuthorizationProvider.None)
        {
            services.AddAuthorization(config =>
            {
                foreach (var permission in Permissions.All)
                {
                    config.AddPolicy(permission, policy => policy.RequireClaim(AuthorizationClaimTypes.Permission, permission));
                }
            });
        }

        return services;
    }

    public static IApplicationBuilder UseAuthorizationService(this IApplicationBuilder app, IConfiguration configuration)
    {
        var authorizationOptions = configuration.GetSection(AuthorizationOptions.SectionKey).Get<AuthorizationOptions>();

        if (authorizationOptions!.Provider != AuthenticationProvider.None)
        {
            app.UseAuthorization();
        }

        return app;
    }
}
