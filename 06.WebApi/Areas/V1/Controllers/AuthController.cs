using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Auth.Commands.Login;
using Smlp.Shared.Auth.Requests;
using Smlp.Application.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Authentication;

namespace Smlp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
public class AuthController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISmlpDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IMediator mediator, ISmlpDbContext context, IJwtTokenService jwtTokenService)
    {
        _mediator = mediator;
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>Login dan dapatkan JWT token</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _mediator.Send(new LoginCommand(request.Username, request.Password));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>Refresh access token menggunakan refresh token</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == request.RefreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow &&
                u.IsActive);

        if (user is null)
            return Unauthorized(new { message = "Refresh token tidak valid atau sudah kadaluarsa." });

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(this);

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken
        });
    }

    /// <summary>Logout dan hapus refresh token</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (userId is not null && Guid.TryParse(userId, out var id))
        {
            var user = await _context.Users.FindAsync(id);
            if (user is not null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync(this);
            }
        }

        return Ok(new { message = "Logout berhasil." });
    }
}
