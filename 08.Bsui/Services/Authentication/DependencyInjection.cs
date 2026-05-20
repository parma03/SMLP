using Microsoft.AspNetCore.Components.Authorization;

namespace Smlp.Bsui.Services.Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthenticationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionKey));
        services.AddScoped<AuthenticationStateProvider, AuthorizedAuthenticationStateProvider>();
        services.AddScoped<AuthorizedAuthenticationStateProvider>();

        return services;
    }

    public static IApplicationBuilder UseAuthenticationService(this IApplicationBuilder app, IConfiguration configuration)
    {
        // JWT Auth untuk Blazor Server - tidak perlu middleware khusus
        // Token dikelola di ProtectedLocalStorage
        return app;
    }
}
