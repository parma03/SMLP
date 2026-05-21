using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.PointRules.Queries;

namespace Smlp.Application.PointRules.Queries;

public class GetPointRuleByListQuery : IRequest<PaginatedListResponse<GetPointRuleResponse>>
{
    public GetPointRuleRequest Request { get; set; } = default!;
}

public class GetPointRuleByListQueryHandler : IRequestHandler<GetPointRuleByListQuery, PaginatedListResponse<GetPointRuleResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetPointRuleByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetPointRuleResponse>> Handle(GetPointRuleByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.PointRules.Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(r => r.Name.Contains(req.Search));

        if (req.IsActive.HasValue)
            query = query.Where(r => r.IsActive == req.IsActive);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(r => new GetPointRuleResponse
            {
                Id = r.Id,
                Name = r.Name,
                SpendAmount = r.SpendAmount,
                PointEarned = r.PointEarned,
                BonusMultiplier = r.BonusMultiplier,
                ValidFrom = r.ValidFrom,
                ValidUntil = r.ValidUntil,
                IsActive = r.IsActive,
                Notes = r.Notes
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetPointRuleResponse> { Items = items, TotalCount = total };
    }
}
