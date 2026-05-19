using Smlp.Bsui.Services.External.Location;

namespace Smlp.Bsui.Services.External;

public static class DependencyInjection
{
    public static IServiceCollection AddExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocationExternalService(configuration);

        return services;
    }
}
