namespace Smlp.Bsui.Services.Authorization.IS4IM;

public static class DependencyInjection
{
    public static IServiceCollection AddIS4IMAuthorizationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IS4IMAuthorizationOptions>(configuration.GetSection(IS4IMAuthorizationOptions.SectionKey));
        services.AddTransient<IAuthorizationService, IS4IMAuthorizationService>();

        return services;
    }
}
