using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Client.Services.BackEnd;
using Smlp.Client.Services.HealthCheck;
using Smlp.Client.Services.Hris;
using Smlp.Client.Services.Iam;
using Smlp.Client.Services.UserInfo;

namespace Smlp.Client;

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
