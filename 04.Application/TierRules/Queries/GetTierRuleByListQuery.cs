using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.TierRules.Queries;

namespace Smlp.Application.TierRules.Queries;

public class GetTierRuleByListQuery : IRequest<PaginatedListResponse<GetTierRuleResponse>>
{
    public GetTierRuleRequest Request { get; set; } = default!;
}

public class GetTierRuleByListQueryHandler : IRequestHandler<GetTierRuleByListQuery, PaginatedListResponse<GetTierRuleResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetTierRuleByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetTierRuleResponse>> Handle(GetTierRuleByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.TierRules.Where(r => !r.IsDeleted);

        if (req.IsActive.HasValue)
            query = query.Where(r => r.IsActive == req.IsActive);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.MinimumLifetimePoints)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
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
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetTierRuleResponse> { Items = items, TotalCount = total };
    }
}
