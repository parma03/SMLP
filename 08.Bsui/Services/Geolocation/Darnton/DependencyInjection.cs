using Smlp.Bsui.Services.Geolocation.Abstractions;

namespace Smlp.Bsui.Services.Geolocation.Darnton;

public static class DependencyInjection
{
    public static IServiceCollection AddDarntonGeolocationService(this IServiceCollection services)
    {
        services.AddScoped<IGeolocationService, BrowserGeolocationService>();
        return services;
    }
}
