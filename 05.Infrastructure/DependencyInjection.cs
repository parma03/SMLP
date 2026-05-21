/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Infrastructure.AppInfo;
using Smlp.Infrastructure.BackgroundJob;
using Smlp.Infrastructure.HealthCheck;
using Smlp.Infrastructure.Persistence;
using Smlp.Infrastructure.Storage;
using Smlp.Infrastructure.DateAndTime;
using Smlp.InfrastructureDomainEvent;
using Smlp.Infrastructure.Otp;
using Smlp.Application.Services.Authorization;
using Smlp.Infrastructure.CurrentUser;
using Smlp.Infrastructure.Authorization;

namespace Smlp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppInfoService(configuration);
        services.AddCurrentUserService();
        services.AddDateAndTimeService();
        services.AddDomainEventService();
        services.AddOtpService();
        services.AddStorage(configuration);

        // Register a stub IAuthorizationService (uses JWT roles, no external auth server)
        services.AddScoped<IAuthorizationService, LocalAuthorizationService>();

        var healthChecksBuilder = services.AddHealthCheckService(configuration);
        services.AddPersistenceService(configuration, healthChecksBuilder);
        services.AddBackgroundJobService(configuration, healthChecksBuilder);

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseHealthCheckService(configuration);
        app.UseBackgroundJobService(configuration);
        return app;
    }
}
