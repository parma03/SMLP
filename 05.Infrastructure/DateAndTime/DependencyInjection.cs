using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.DateAndTime;

namespace Smlp.Infrastructure.DateAndTime;

public static class DependencyInjection
{
    public static IServiceCollection AddDateAndTimeService(this IServiceCollection services)
    {
        services.AddTransient<IDateAndTimeService, DateAndTimeService>();

        return services;
    }
}
