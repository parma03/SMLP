using Microsoft.Extensions.DependencyInjection;
using Psmp.Application.Services.DateAndTime;

namespace Psmp.InfrastructureDateAndTime;

public static class DependencyInjection
{
    public static IServiceCollection AddDateAndTimeService(this IServiceCollection services)
    {
        services.AddTransient<IDateAndTimeService, DateAndTimeService>();

        return services;
    }
}
