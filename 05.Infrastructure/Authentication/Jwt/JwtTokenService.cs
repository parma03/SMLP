/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Smlp.Application.Services.Authentication;
using Smlp.Base.User.Enums;
using Smlp.Domain.Entities.Master;

namespace Smlp.Infrastructure.Authentication.Jwt;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Map UserRole enum ke string role untuk policy-based auth
        var roleString = user.Role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Owner => "Owner",
            UserRole.Kasir => "Kasir",
            UserRole.Pelanggan => "Customer",
            _ => "Customer"
        };

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.EmailAddress),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new("username", user.Username),
            new("displayname", user.DisplayName),
            new("role", roleString),
            new(ClaimTypes.Role, roleString),           // For ASP.NET Core role-based auth
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Guid? ValidateRefreshToken(string refreshToken)
    {
        // Refresh token validation is handled at application layer
        return null;
    }
}
