using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.PointRules.Commands.Delete;

public class DeletePointRuleCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeletePointRuleCommandHandler : IRequestHandler<DeletePointRuleCommand>
{
    private readonly ISmlpDbContext _context;

    public DeletePointRuleCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<Unit> Handle(DeletePointRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.PointRules
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(PointRule), request.Id);

        _context.PointRules.Remove(rule);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
