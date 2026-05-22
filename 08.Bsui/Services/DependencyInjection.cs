using Smlp.Bsui.Common.Services.Notifications;
using Smlp.Bsui.Services.AppInfo;
using Smlp.Bsui.Services.Authentication;
using Smlp.Bsui.Services.Authorization;
using Smlp.Bsui.Services.External;
using Smlp.Bsui.Services.FrontEnd;
using Smlp.Bsui.Services.Geolocation;
using Smlp.Bsui.Services.Security;
using Smlp.Bsui.Services.Telemetry;
using Smlp.Bsui.Services.UI;

namespace Smlp.Bsui.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddBsui(this IServiceCollection services, IConfiguration configuration)
    {
        #region AppInfo

        services.AddAppInfoService(configuration);

        #endregion AppInfo

        #region Authentication

        services.AddAuthenticationService(configuration);

        #endregion Authentication

        #region Authorization

        services.AddAuthorizationService(configuration);

        #endregion Authorization

        #region External

        services.AddExternalService(configuration);

        #endregion External

        #region Front End

        services.AddFrontEndService(configuration);

        #endregion Front End

        #region Geolocation

        services.AddGeolocationService(configuration);

        #endregion Geolocation

        #region Security

        services.AddSecurityService();

        #endregion Security

        #region Telemetry

        services.AddTelemetryService(configuration);

        #endregion Telemetry

        #region User Interface

        services.AddUIService(configuration);

        #endregion User Interface

        #region Notifications

        services.AddScoped<NotificationService>();

        #endregion Notifications

        #region AppInfo

        services.AddAppInfoService(configuration);

        #endregion AppInfo

        #region Authentication

        services.AddAuthenticationService(configuration);

        #endregion Authentication

        #region Authorization

        services.AddAuthorizationService(configuration);

        #endregion Authorization

        #region External

        services.AddExternalService(configuration);

        #endregion External

        #region Front End

        services.AddFrontEndService(configuration);

        #endregion Front End

        #region Geolocation

        services.AddGeolocationService(configuration);

        #endregion Geolocation

        #region Security

        services.AddSecurityService();

        #endregion Security

        #region Telemetry

        services.AddTelemetryService(configuration);

        #endregion Telemetry

        #region User Interface

        services.AddUIService(configuration);

        #endregion User Interface

        return services;
    }

    public static IApplicationBuilder UseBsui(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseFrontEndService(configuration);

        return app;
    }
}
