using Smlp.Bsui.Services.Logging;
using Smlp.Shared.Common.Constants;

namespace Smlp.Bsui.Services.Telemetry.None;

public static class DependencyInjection
{
    public static IServiceCollection AddNoneTelemetryService(this IServiceCollection services)
    {
        LoggingHelper
            .CreateLogger()
            .LogWarning("{ServiceName} is set to None.", $"{nameof(Telemetry)} {CommonDisplayTextFor.Service}");

        return services;
    }
}
