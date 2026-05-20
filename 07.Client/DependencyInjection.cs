/*
 * Sistem Manajemen Loyalitas Pelanggan (SMLP)
 * Copyright (c) 2025-2026
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Client.Services.BackEnd;
using Smlp.Client.Services.HealthCheck;
using Smlp.Client.Services.UserInfo;

namespace Smlp.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthCheckService();
        services.AddBackEndService(configuration);
        services.AddUserInfoService();

        return services;
    }
}
