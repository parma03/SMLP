using Smlp.Bsui.Services.Logging;
using Smlp.Shared.Common.Constants;

namespace Smlp.Bsui.Services.Authentication.None;

/// <summary>
/// Mode None: untuk development tanpa autentikasi.
/// JWT token di-bypass dan semua user dianggap Admin.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddNoneAuthenticationService(this IServiceCollection services)
    {
        LoggingHelper
            .CreateLogger()
            .LogWarning("{ServiceName} is set to None (Development Mode).", $"{nameof(Authentication)} {CommonDisplayTextFor.Service}");

        return services;
    }
}
