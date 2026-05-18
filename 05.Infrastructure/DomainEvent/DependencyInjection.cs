using Microsoft.Extensions.DependencyInjection;
using Psmp.Application.Services.DomainEvent;

namespace Psmp.InfrastructureDomainEvent;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainEventService(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventService, DomainEventService>();

        return services;
    }
}
