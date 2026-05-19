using Psmp.Bsui.Services.AppInfo;
using Psmp.Bsui.Services.Authentication;
using Psmp.Bsui.Services.Authorization;
using Psmp.Bsui.Services.External;
using Psmp.Bsui.Services.FrontEnd;
using Psmp.Bsui.Services.Geolocation;
using Psmp.Bsui.Services.Security;
using Psmp.Bsui.Services.Telemetry;
using Psmp.Bsui.Services.UI;

namespace Psmp.Bsui.Services;

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

        return services;
    }

    public static IApplicationBuilder UseBsui(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseFrontEndService(configuration);

        return app;
    }
}
