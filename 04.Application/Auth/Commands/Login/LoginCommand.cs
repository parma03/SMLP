using MediatR;
using Smlp.Application.Services.Authentication;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Auth.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Smlp.Application.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(ISmlpDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Username atau password salah.");

        // Password check (hashed)
        var passwordHash = ComputeSha256Hash(request.Password);
        if (user.PasswordHash != passwordHash)
            throw new UnauthorizedAccessException("Username atau password salah.");

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Store refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 480 * 60,
            UserId = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString()
        };
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}
