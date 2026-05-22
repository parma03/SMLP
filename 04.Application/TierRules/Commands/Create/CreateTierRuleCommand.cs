using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.TierRules.Commands.Create;

namespace Smlp.Application.TierRules.Commands.Create;

public class CreateTierRuleCommand : IRequest<CommonCreateResponse>
{
    public CreateTierRuleRequest Request { get; set; } = default!;
}

public class CreateTierRuleCommandHandler : IRequestHandler<CreateTierRuleCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;

    public CreateTierRuleCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonCreateResponse> Handle(CreateTierRuleCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var exists = await _context.TierRules.AnyAsync(r => r.Name == req.Name && !r.IsDeleted, cancellationToken);
        if (exists)
            throw new AlreadyExistsExceptions(nameof(TierRule), req.Name);

        var rule = new TierRule
        {
            Name = req.Name,
            Tier = req.Tier,
            MinimumLifetimePoints = req.MinimumLifetimePoints,
            MaximumLifetimePoints = req.MaximumLifetimePoints,
            PointMultiplier = req.PointMultiplier,
            BenefitDescription = req.BenefitDescription,
            BadgeColor = req.BadgeColor,
            IsActive = req.IsActive
        };

        await _context.TierRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonCreateResponse { Id = rule.Id };
    }
}
