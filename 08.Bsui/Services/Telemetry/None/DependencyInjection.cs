using Psmp.Bsui.Services.Logging;
using Psmp.Shared.Common.Constants;

namespace Psmp.Bsui.Services.Telemetry.None;

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
