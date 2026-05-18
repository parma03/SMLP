using Microsoft.Extensions.DependencyInjection;
using Psmp.Application.Services.BackgroundJob;

namespace Psmp.Infrastructure.BackgroundJob.None;

public static class DependencyInjection
{
    public static IServiceCollection AddNoneBackgroundJobService(this IServiceCollection services)
    {
        services.AddTransient<IBackgroundJobService, NoneBackgroundJobService>();

        return services;
    }
}
