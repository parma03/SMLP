using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Smlp.Bsui.Services.Telemetry.ApplicationInsights;

public class CustomTelemetryProcessor : ITelemetryProcessor
{
    private ITelemetryProcessor Next { get; set; }

    public CustomTelemetryProcessor(ITelemetryProcessor next)
    {
        Next = next;
    }

    public void Process(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry)
        {
            return;
        }

        Next.Process(telemetry);
    }
}
