using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Auth.Responses;

public class LoginResponse : Response
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public int ExpiresIn { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Role { get; set; } = default!;
}
