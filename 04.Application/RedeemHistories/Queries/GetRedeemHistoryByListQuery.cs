using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.RedeemHistories.Queries;

namespace Smlp.Application.RedeemHistories.Queries;

public class GetRedeemHistoryByListQuery : IRequest<PaginatedListResponse<GetRedeemHistoryResponse>>
{
    public GetRedeemHistoryRequest Request { get; set; } = default!;
}

public class GetRedeemHistoryByListQueryHandler : IRequestHandler<GetRedeemHistoryByListQuery, PaginatedListResponse<GetRedeemHistoryResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetRedeemHistoryByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetRedeemHistoryResponse>> Handle(GetRedeemHistoryByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.RedeemHistories
            .Include(r => r.Customer)
            .Include(r => r.Reward)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(r =>
                r.RedeemCode.Contains(req.Search) ||
                r.Customer.FullName.Contains(req.Search) ||
                r.Customer.MemberCode.Contains(req.Search) ||
                r.RewardNameSnapshot.Contains(req.Search));

        if (req.CustomerId.HasValue)
            query = query.Where(r => r.CustomerId == req.CustomerId);

        if (req.Status.HasValue)
            query = query.Where(r => r.Status == req.Status);

        if (req.DateFrom.HasValue)
            query = query.Where(r => r.RedeemDate >= req.DateFrom);

        if (req.DateTo.HasValue)
            query = query.Where(r => r.RedeemDate <= req.DateTo);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.RedeemDate)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(r => new GetRedeemHistoryResponse
            {
                Id = r.Id,
                RedeemCode = r.RedeemCode,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName,
                CustomerMemberCode = r.Customer.MemberCode,
                RewardId = r.RewardId,
                RewardName = r.RewardNameSnapshot,
                RedeemDate = r.RedeemDate,
                PointsUsed = r.PointsUsed,
                Status = r.Status,
                VerifiedBy = r.VerifiedBy,
                VerifiedAt = r.VerifiedAt,
                Notes = r.Notes,
                PointCostSnapshot = r.PointCostSnapshot,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetRedeemHistoryResponse> { Items = items, TotalCount = total };
    }
}
