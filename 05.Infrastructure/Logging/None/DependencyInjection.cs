using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Psmp.Infrastructure.Logging.None;

public static class DependencyInjection
{
    public static IHostBuilder UseNoneLoggingService(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureLogging((hostContext, loggingBuilder) =>
        {
            loggingBuilder.AddConfiguration(hostContext.Configuration.GetSection($"{nameof(Logging)}:{nameof(None)}"));
            loggingBuilder.ClearProviders();
            loggingBuilder.AddDebug();
            loggingBuilder.AddSimpleConsole(LoggingHelper.SimpleConsoleOptions);
        });

        return hostBuilder;
    }
}
