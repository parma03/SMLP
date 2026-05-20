using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Services.Authentication;

public interface IJwtTokenService
{
    public string GenerateAccessToken(User user);
    public string GenerateRefreshToken();
    public Guid? ValidateRefreshToken(string refreshToken);
}
