using Microsoft.Extensions.Logging.Console;

namespace Psmp.Bsui.Services.Logging;

public static class LoggingHelper
{
    public static readonly Action<SimpleConsoleFormatterOptions> SimpleConsoleOptions = new(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "[HH:mm:ss] ";
    });

    public static ILogger CreateLogger()
    {
        return LoggerFactory
            .Create(builder => builder.AddSimpleConsole(SimpleConsoleOptions))
            .CreateLogger(nameof(Services));
    }
}
