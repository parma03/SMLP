using Microsoft.Extensions.DependencyInjection;
using Psmp.Application.Services.Otp;

namespace Psmp.InfrastructureOtp;

public static class DependencyInjection
{
    public static IServiceCollection AddOtpService(this IServiceCollection services)
    {
        services.AddTransient<IOtpService, OtpService>();

        return services;
    }
}
