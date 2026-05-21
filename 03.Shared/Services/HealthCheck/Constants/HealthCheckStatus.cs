namespace Smlp.Shared.Services.HealthCheck.Constants;

public static class HealthCheckStatus
{
    public const string Loading = nameof(Loading);
    public const string Healthy = nameof(Healthy);
    public const string Unhealthy = nameof(Unhealthy);
    public const string Degraded = nameof(Degraded);
    public const string Unknown = nameof(Unknown);
}
