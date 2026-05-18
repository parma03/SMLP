using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Psmp.Client.Services.BackEnd;
using Psmp.Client.Services.HealthCheck;
using Psmp.Client.Services.Hris;
using Psmp.Client.Services.Iam;
using Psmp.Client.Services.UserInfo;

namespace Psmp.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthCheckService();
        services.AddBackEndService(configuration);
        services.AddUserInfoService();
        services.AddHrisService(configuration);
        services.AddIamService(configuration);

        return services;
    }
}
