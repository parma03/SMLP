/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Infrastructure.AppInfo;
using Smlp.Infrastructure.BackgroundJob;
using Smlp.Infrastructure.BackgroundJob.Jobs;
using Smlp.Infrastructure.HealthCheck;
using Smlp.Infrastructure.Persistence;
using Smlp.Infrastructure.Storage;
using Smlp.Infrastructure.DateAndTime;
using Smlp.InfrastructureDomainEvent;
using Smlp.Infrastructure.Otp;
using Smlp.Application.Services.Authorization;
using Smlp.Application.BackgroundJobs;
using Smlp.Infrastructure.CurrentUser;
using Smlp.Infrastructure.Authorization;
using Smlp.Application.Services.Email;
using Smlp.Application.Services.Email.Models;
using Smlp.Application.Services.WhatsApp;
using Smlp.Application.Services.WhatsApp.Models;
using Smlp.Infrastructure.Notifications.Email;
using Smlp.Infrastructure.Notifications.WhatsApp;

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

        // Background Jobs
        services.AddScoped<IExpiredPointsJob, ExpiredPointsJob>();

        // Notification Services
        services.AddNotificationServices(configuration);

        var healthChecksBuilder = services.AddHealthCheckService(configuration);
        services.AddPersistenceService(configuration, healthChecksBuilder);
        services.AddBackgroundJobService(configuration, healthChecksBuilder);

        return services;
    }

    private static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Email
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        var emailProvider = configuration["Email:Provider"] ?? "None";
        if (emailProvider is "Gmail" or "Smtp")
            services.AddScoped<IEmailService, SmtpEmailService>();
        else
            services.AddScoped<IEmailService, NullEmailService>();

        // WhatsApp
        services.Configure<WhatsAppSettings>(configuration.GetSection("WhatsApp"));
        var waProvider = configuration["WhatsApp:Provider"] ?? "None";
        if (waProvider == "Fonnte")
        {
            services.AddHttpClient("Fonnte");
            services.AddScoped<IWhatsAppService, FonnteWhatsAppService>();
        }
        else
        {
            services.AddScoped<IWhatsAppService, NullWhatsAppService>();
        }

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseHealthCheckService(configuration);
        app.UseBackgroundJobService(configuration);
        return app;
    }
}
