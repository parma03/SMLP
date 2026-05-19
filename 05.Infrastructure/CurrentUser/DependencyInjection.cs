using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.CurrentUser;

namespace Smlp.InfrastructureCurrentUser;

public static class DependencyInjection
{
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
