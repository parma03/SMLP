using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Common.Exceptions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Users.Commands.Update;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Users.Commands.Update;

public class UpdateUserCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdateUserRequest Request { get; set; } = default!;
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;

    public UpdateUserCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonUpdateResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        var req = request.Request;
        user.DisplayName = req.DisplayName;
        user.Role = req.Role;
        user.PhoneNumber = req.PhoneNumber;
        user.EmailAddress = req.EmailAddress;
        user.IsActive = req.IsActive;

        await _context.SaveChangesAsync(this, cancellationToken);
        return new CommonUpdateResponse { Id = user.Id };
    }
}
