namespace Smlp.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionKey = "Jwt";

    public string SecretKey { get; set; } = default!;
    public string Issuer { get; set; } = "SMLP";
    public string Audience { get; set; } = "SMLP_Client";
    public int ExpiryInMinutes { get; set; } = 480; // 8 jam
    public int RefreshExpiryInDays { get; set; } = 7;
}
