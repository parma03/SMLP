using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Reports.Queries.GetRedeemReport;

namespace Smlp.Application.Reports.Queries;

public class GetRedeemReportQuery : IRequest<PaginatedListResponse<GetRedeemReportResponse>>
{
    public GetRedeemReportRequest Request { get; set; } = default!;
}

public class GetRedeemReportQueryHandler : IRequestHandler<GetRedeemReportQuery, PaginatedListResponse<GetRedeemReportResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetRedeemReportQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetRedeemReportResponse>> Handle(GetRedeemReportQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.RedeemHistories
            .Include(r => r.Customer)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(r => r.RedeemCode.Contains(req.Search)
                || r.Customer.FullName.Contains(req.Search)
                || r.Customer.MemberCode.Contains(req.Search));

        if (req.CustomerId.HasValue)
            query = query.Where(r => r.CustomerId == req.CustomerId);

        if (req.DateFrom.HasValue)
            query = query.Where(r => r.RedeemDate >= req.DateFrom);

        if (req.DateTo.HasValue)
            query = query.Where(r => r.RedeemDate <= req.DateTo);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.RedeemDate)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(r => new GetRedeemReportResponse
            {
                Id = r.Id,
                RedeemCode = r.RedeemCode,
                CustomerName = r.Customer.FullName,
                MemberCode = r.Customer.MemberCode,
                RewardName = r.RewardNameSnapshot,
                RedeemDate = r.RedeemDate,
                PointsUsed = r.PointsUsed,
                Status = r.Status.ToString(),
                VerifiedBy = r.VerifiedBy
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetRedeemReportResponse> { Items = items, TotalCount = total };
    }
}
