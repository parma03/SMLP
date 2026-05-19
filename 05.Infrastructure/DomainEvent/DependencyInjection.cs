using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.DomainEvent;

namespace Smlp.InfrastructureDomainEvent;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainEventService(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventService, DomainEventService>();

        return services;
    }
}
