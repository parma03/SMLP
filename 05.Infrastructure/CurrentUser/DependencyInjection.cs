using Microsoft.Extensions.DependencyInjection;
using Psmp.Application.Services.CurrentUser;

namespace Psmp.InfrastructureCurrentUser;

public static class DependencyInjection
{
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
