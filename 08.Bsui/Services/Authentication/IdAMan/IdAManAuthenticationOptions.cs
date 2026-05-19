namespace Smlp.Bsui.Services.Authentication.IdAMan;

public class IdAManAuthenticationOptions
{
    public static readonly string SectionKey = $"{nameof(Authentication)}:{nameof(IdAMan)}";

    public Redirect Redirect { get; set; } = default!;
    public Proxy Proxy { get; set; } = default!;
    public string AuthorityUrl { get; set; } = default!;
    public Endpoints Endpoints { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string ApiAudienceScope { get; set; } = default!;

    public string TokenUrl => $"{AuthorityUrl}{Endpoints.Token}";
    public string HealthCheckUrl => $"{AuthorityUrl}{Endpoints.HealthCheck}";
}

public class Redirect
{
    public bool Enabled { get; set; }
    public string Url { get; set; } = default!;
}

public class Proxy
{
    public bool Enabled { get; set; }
    public IList<string> Hosts { get; set; } = new List<string>();
}

public class Endpoints
{
    public string Token { get; set; } = default!;
    public string HealthCheck { get; set; } = default!;
}
