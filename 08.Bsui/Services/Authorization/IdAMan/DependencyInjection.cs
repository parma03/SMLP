namespace Smlp.Bsui.Services.Authorization.IdAMan;

public static class DependencyInjection
{
    public static IServiceCollection AddIdAManAuthorizationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdAManAuthorizationOptions>(configuration.GetSection(IdAManAuthorizationOptions.SectionKey));
        services.AddTransient<IAuthorizationService, IdAManAuthorizationService>();

        return services;
    }
}
