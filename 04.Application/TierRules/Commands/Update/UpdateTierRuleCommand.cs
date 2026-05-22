using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.TierRules.Commands.Update;

namespace Smlp.Application.TierRules.Commands.Update;

public class UpdateTierRuleCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdateTierRuleRequest Request { get; set; } = default!;
}

public class UpdateTierRuleCommandHandler : IRequestHandler<UpdateTierRuleCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;

    public UpdateTierRuleCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonUpdateResponse> Handle(UpdateTierRuleCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var rule = await _context.TierRules
            .Where(r => r.Id == request.Id && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(TierRule), request.Id);

        rule.Name = req.Name;
        rule.Tier = req.Tier;
        rule.MinimumLifetimePoints = req.MinimumLifetimePoints;
        rule.MaximumLifetimePoints = req.MaximumLifetimePoints;
        rule.PointMultiplier = req.PointMultiplier;
        rule.BenefitDescription = req.BenefitDescription;
        rule.BadgeColor = req.BadgeColor;
        rule.IsActive = req.IsActive;

        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonUpdateResponse { Id = rule.Id };
    }
}
