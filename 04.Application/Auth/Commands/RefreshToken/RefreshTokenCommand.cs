using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Authentication;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Auth.Requests;
using Smlp.Shared.Auth.Responses;

namespace Smlp.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : RefreshTokenRequest, IRequest<RefreshTokenResponse>
{
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(ISmlpDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == request.RefreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow &&
                u.IsActive, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Refresh token tidak valid atau sudah kadaluarsa.");

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 480 * 60
        };
    }
}
