namespace Smlp.Bsui.Services.AppInfo;

public static class DependencyInjection
{
    public static IServiceCollection AddAppInfoService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppInfoOptions>(configuration.GetSection(AppInfoOptions.SectionKey));

        return services;
    }
}
