using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Smlp.Shared.Documents.Options;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DocumentOptions>(configuration.GetSection(DocumentOptions.SectionKey));

        return services;
    }
}
