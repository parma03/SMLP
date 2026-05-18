using Psmp.WebApi.Services.BackEnd;
using Psmp.WebApi.Services.Documentation;

namespace Psmp.WebApi.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBackEndService(configuration);
        services.AddDocumentationService(configuration);

        return services;
    }
}
