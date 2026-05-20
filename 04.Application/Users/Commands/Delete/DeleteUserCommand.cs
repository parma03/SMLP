using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Common.Exceptions;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Users.Commands.Delete;

public class DeleteUserCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly ISmlpDbContext _context;

    public DeleteUserCommandHandler(ISmlpDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        _context.Users.Remove(user);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
