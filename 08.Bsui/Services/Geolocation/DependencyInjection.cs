using Smlp.Bsui.Services.Geolocation.Darnton;
using Smlp.Bsui.Services.Geolocation.None;

namespace Smlp.Bsui.Services.Geolocation;

public static class DependencyInjection
{
    public static IServiceCollection AddGeolocationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeolocationOptions>(configuration.GetSection(GeolocationOptions.SectionKey));

        var geolocationOptions = configuration.GetSection(GeolocationOptions.SectionKey).Get<GeolocationOptions>();

        if (geolocationOptions.Enabled)
        {
            services.AddDarntonGeolocationService();
        }
        else
        {
            services.AddNoneGeolocationService();
        }

        return services;
    }
}
