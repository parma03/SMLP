namespace Psmp.Bsui.Services.Authorization.None;

public static class DependencyInjection
{
    public static IServiceCollection AddNoneAuthorizationService(this IServiceCollection services)
    {
        services.AddTransient<IAuthorizationService, NoneAuthorizationService>();

        return services;
    }
}
