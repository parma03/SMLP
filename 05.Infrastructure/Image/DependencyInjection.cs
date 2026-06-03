using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Image;

namespace Smlp.Infrastructure.Image;

public static class DependencyInjection
{
    public static IServiceCollection AddImageCompressionService(this IServiceCollection services)
    {
        services.AddScoped<IImageCompressionService, ImageSharpCompressionService>();
        return services;
    }
}
