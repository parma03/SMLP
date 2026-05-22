using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.TierRules.Commands.Delete;

public class DeleteTierRuleCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeleteTierRuleCommandHandler : IRequestHandler<DeleteTierRuleCommand, Unit>
{
    private readonly ISmlpDbContext _context;

    public DeleteTierRuleCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteTierRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.TierRules
            .Where(r => r.Id == request.Id && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(TierRule), request.Id);

        _context.TierRules.Remove(rule);
        await _context.SaveChangesAsync(this, cancellationToken);

        return Unit.Value;
    }
}
