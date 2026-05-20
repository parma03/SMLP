using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Rewards.Commands.Delete;

public class DeleteRewardCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteRewardCommandHandler : IRequestHandler<DeleteRewardCommand>
{
    private readonly ISmlpDbContext _context;

    public DeleteRewardCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context.Rewards
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Reward), request.Id);

        _context.Rewards.Remove(reward);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
