namespace Smlp.Bsui.Services.Authorization;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorizationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthorizationOptions>(configuration.GetSection(AuthorizationOptions.SectionKey));
        services.AddAuthorization(config =>
        {
            config.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            config.AddPolicy("Owner", policy => policy.RequireRole("Admin", "Owner"));
            config.AddPolicy("Kasir", policy => policy.RequireRole("Admin", "Owner", "Kasir"));
        });

        return services;
    }

    public static IApplicationBuilder UseAuthorizationService(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseAuthorization();
        return app;
    }
}
