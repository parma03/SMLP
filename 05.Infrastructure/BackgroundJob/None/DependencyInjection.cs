using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.BackgroundJob;

namespace Smlp.Infrastructure.BackgroundJob.None;

public static class DependencyInjection
{
    public static IServiceCollection AddNoneBackgroundJobService(this IServiceCollection services)
    {
        services.AddTransient<IBackgroundJobService, NoneBackgroundJobService>();

        return services;
    }
}
