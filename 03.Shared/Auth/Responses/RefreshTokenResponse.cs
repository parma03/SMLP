using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Auth.Responses;

public class RefreshTokenResponse : Response
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public int ExpiresIn { get; set; }
}
