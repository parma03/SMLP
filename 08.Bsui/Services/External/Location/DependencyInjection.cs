namespace Smlp.Bsui.Services.External.Location;

public static class DependencyInjection
{
    public static IServiceCollection AddLocationExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocationExternalOptions>(configuration.GetSection(LocationExternalOptions.SectionKey));
        services.AddTransient<LocationExternalService>();

        return services;
    }
}
