using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Smlp.Infrastructure.AppInfo;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Services.Logging.Constants;
using Serilog;
using Serilog.Debugging;
using Serilog.Settings.Configuration;

namespace Smlp.Infrastructure.Logging.Serilog;

public static class DependencyInjection
{
    public static IHostBuilder UseSerilogLoggingService(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((hostBuilderContext, loggerConfiguration) => loggerConfiguration.ConfigureSerilog(hostBuilderContext.Configuration));
        SelfLog.Enable(message => Console.WriteLine(message));

        return hostBuilder;
    }

    public static LoggerConfiguration ConfigureSerilog(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var appInfoOptions = configuration.GetSection(AppInfoOptions.SectionKey).Get<AppInfoOptions>();

        return loggerConfiguration
            .ReadFrom.Configuration(configuration, new Serilog.Settings.Configuration.ConfigurationReaderOptions { SectionName = $"{nameof(Logging)}:{nameof(Serilog)}" })
            .Enrich.WithProperty(PropertyNameFor.Version, CommonValueFor.EntryAssemblyVersion!)
            .Enrich.WithProperty(PropertyNameFor.ApplicationName, appInfoOptions?.FullName ?? "SMLP")
            .Enrich.WithProperty(PropertyNameFor.AssemblyName, CommonValueFor.EntryAssemblySimpleName!)
            .Enrich.WithProperty(PropertyNameFor.EnvironmentName, CommonValueFor.EnvironmentName);
    }
}
