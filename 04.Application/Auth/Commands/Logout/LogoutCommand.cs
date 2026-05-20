using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.CurrentUser;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;

namespace Smlp.Application.Auth.Commands.Logout;

public class LogoutCommand : IRequest<SuccessResponse>
{
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, SuccessResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public LogoutCommandHandler(ISmlpDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SuccessResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId.HasValue)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

            if (user is not null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync(this, cancellationToken);
            }
        }

        return new SuccessResponse();
    }
}
