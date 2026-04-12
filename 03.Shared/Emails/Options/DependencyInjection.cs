using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Smlp.Shared.Emails.Options;

public static class DependencyInjection
{
    public static IServiceCollection AddEmailOptions(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration != null)
        {
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionKey));
        }

        return services;
    }
}
