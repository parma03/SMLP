/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Smlp.Bsui.Services.Authorization.None;

namespace Smlp.Bsui.Services.Authorization;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorizationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthorizationOptions>(configuration.GetSection(AuthorizationOptions.SectionKey));

        // SMLP: Role-based authorization policies
        services.AddAuthorization(config =>
        {
            // Role policies
            config.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            config.AddPolicy("Owner", policy => policy.RequireRole("Admin", "Owner"));
            config.AddPolicy("Kasir", policy => policy.RequireRole("Admin", "Owner", "Kasir"));
            config.AddPolicy("Customer", policy => policy.RequireRole("Admin", "Owner", "Kasir", "Customer"));

            // Feature policies
            config.AddPolicy("CanInputTransaction", policy => policy.RequireRole("Admin", "Kasir"));
            config.AddPolicy("CanViewAnalytics", policy => policy.RequireRole("Admin", "Owner"));
            config.AddPolicy("CanManageRewards", policy => policy.RequireRole("Admin", "Owner"));
            config.AddPolicy("CanManageMasterData", policy => policy.RequireRole("Admin"));
        });

        // No-op authorization service (SMLP uses JWT role-based, not external provider)
        services.AddScoped<IAuthorizationService, NoneAuthorizationService>();

        return services;
    }

    public static IApplicationBuilder UseAuthorizationService(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseAuthorization();
        return app;
    }
}
