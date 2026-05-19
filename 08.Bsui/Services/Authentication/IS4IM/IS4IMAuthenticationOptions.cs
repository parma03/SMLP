namespace Smlp.Bsui.Services.Authentication.IS4IM;

public class IS4IMAuthenticationOptions
{
    public static readonly string SectionKey = $"{nameof(Authentication)}:{nameof(IS4IM)}";

    public string AuthorityUrl { get; set; } = default!;
    public Endpoints Endpoints { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string ApiAudienceScope { get; set; } = default!;

    public string TokenUrl => $"{AuthorityUrl}{Endpoints.Token}";
    public string HealthCheckUrl => $"{AuthorityUrl}{Endpoints.HealthCheck}";
}

public class Endpoints
{
    public string Token { get; set; } = default!;
    public string HealthCheck { get; set; } = default!;
}
