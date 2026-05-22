using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Master.TierRules.Queries;

namespace Smlp.Application.TierRules.Queries;

public class GetTierRuleByIdQuery : IRequest<GetTierRuleResponse>
{
    public Guid Id { get; set; }
}

public class GetTierRuleByIdQueryHandler : IRequestHandler<GetTierRuleByIdQuery, GetTierRuleResponse>
{
    private readonly ISmlpDbContext _context;

    public GetTierRuleByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetTierRuleResponse> Handle(GetTierRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _context.TierRules
            .Where(r => r.Id == request.Id && !r.IsDeleted)
            .Select(r => new GetTierRuleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Tier = r.Tier,
                MinimumLifetimePoints = r.MinimumLifetimePoints,
                MaximumLifetimePoints = r.MaximumLifetimePoints,
                PointMultiplier = r.PointMultiplier,
                BenefitDescription = r.BenefitDescription,
                BadgeColor = r.BadgeColor,
                IsActive = r.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(TierRule), request.Id);

        return rule;
    }
}
