using Smlp.WebApi.Services.BackEnd;
using Smlp.WebApi.Services.Documentation;

namespace Smlp.WebApi.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBackEndService(configuration);
        services.AddDocumentationService(configuration);

        return services;
    }
}
