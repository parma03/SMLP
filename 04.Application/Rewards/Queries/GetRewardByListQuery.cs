using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Rewards.Queries;

namespace Smlp.Application.Rewards.Queries;

public class GetRewardByListQuery : IRequest<PaginatedListResponse<GetRewardResponse>>
{
    public GetRewardRequest Request { get; set; } = default!;
}

public class GetRewardByListQueryHandler : IRequestHandler<GetRewardByListQuery, PaginatedListResponse<GetRewardResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetRewardByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetRewardResponse>> Handle(GetRewardByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.Rewards.Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(r => r.Name.Contains(req.Search));

        if (req.IsActive.HasValue)
            query = query.Where(r => r.IsActive == req.IsActive);

        if (req.Type.HasValue)
            query = query.Where(r => r.Type == req.Type);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(r => new GetRewardResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Type = r.Type,
                PointCost = r.PointCost,
                DiscountAmount = r.DiscountAmount,
                DiscountPercent = r.DiscountPercent,
                FreeItemName = r.FreeItemName,
                StockLimit = r.StockLimit,
                RedeemedCount = r.RedeemedCount,
                ValidFrom = r.ValidFrom,
                ValidUntil = r.ValidUntil,
                IsActive = r.IsActive,
                MinimumTier = r.MinimumTier,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetRewardResponse> { Items = items, TotalCount = total };
    }
}
