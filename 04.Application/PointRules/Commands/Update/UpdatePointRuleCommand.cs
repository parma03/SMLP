using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.PointRules.Commands.Update;

namespace Smlp.Application.PointRules.Commands.Update;

public class UpdatePointRuleCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdatePointRuleRequest Request { get; set; } = default!;
}

public class UpdatePointRuleCommandHandler : IRequestHandler<UpdatePointRuleCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;

    public UpdatePointRuleCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonUpdateResponse> Handle(UpdatePointRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.PointRules
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(PointRule), request.Id);

        var req = request.Request;
        rule.Name = req.Name;
        rule.SpendAmount = req.SpendAmount;
        rule.PointEarned = req.PointEarned;
        rule.BonusMultiplier = req.BonusMultiplier;
        rule.ValidFrom = req.ValidFrom;
        rule.ValidUntil = req.ValidUntil;
        rule.IsActive = req.IsActive;
        rule.Notes = req.Notes;

        await _context.SaveChangesAsync(this, cancellationToken);
        return new CommonUpdateResponse { Id = rule.Id };
    }
}
