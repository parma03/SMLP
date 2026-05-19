using Smlp.Bsui.Services.Logging;
using Smlp.Shared.Common.Constants;

namespace Smlp.Bsui.Services.Authentication.None;

public static class DependencyInjection
{
    public static IServiceCollection AddNoneAuthenticationService(this IServiceCollection services)
    {
        LoggingHelper
            .CreateLogger()
            .LogWarning("{ServiceName} is set to None.", $"{nameof(Authentication)} {CommonDisplayTextFor.Service}");

        return services;
    }
}
