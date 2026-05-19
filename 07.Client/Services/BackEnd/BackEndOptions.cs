namespace Smlp.Client.Services.BackEnd;

public class BackEndOptions
{
    public const string SectionKey = nameof(BackEnd);

    public string BaseUrl { get; set; } = default!;
    public HealthCheck HealthCheck { get; set; } = default!;

    public string HealthCheckApiUrl => $"{BaseUrl}{HealthCheck.Endpoint}";
    public string HealthCheckUIUrl => $"{BaseUrl}{HealthCheck.UI.Endpoint}";
}

public class HealthCheck
{
    public string Endpoint { get; set; } = default!;
    public UI UI { get; set; } = default!;
}

public class UI
{
    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = default!;
}
