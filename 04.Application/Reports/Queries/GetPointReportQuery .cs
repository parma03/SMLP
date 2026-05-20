using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Reports.Queries.GetPointReport;

namespace Smlp.Application.Reports.Queries;

public class GetPointReportQuery : IRequest<PaginatedListResponse<GetPointReportResponse>>
{
    public GetPointReportRequest Request { get; set; } = default!;
}

public class GetPointReportQueryHandler : IRequestHandler<GetPointReportQuery, PaginatedListResponse<GetPointReportResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetPointReportQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetPointReportResponse>> Handle(GetPointReportQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        // Combine: Earn from transactions, Redeem from redeemHistories, Adjustments
        var earnItems = await _context.Transactions
            .Include(t => t.Customer)
            .Where(t => !t.IsDeleted && t.Status == TransactionStatus.Completed && t.PointsEarned > 0)
            .Where(t => !req.CustomerId.HasValue || t.CustomerId == req.CustomerId)
            .Where(t => !req.DateFrom.HasValue || t.TransactionDate >= req.DateFrom)
            .Where(t => !req.DateTo.HasValue || t.TransactionDate <= req.DateTo)
            .Select(t => new GetPointReportResponse
            {
                Id = t.Id,
                CustomerName = t.Customer.FullName,
                MemberCode = t.Customer.MemberCode,
                ActivityType = "Earn",
                Points = t.PointsEarned,
                Description = $"Transaksi {t.TransactionCode}",
                ActivityDate = t.TransactionDate,
                BalanceAfter = 0
            })
            .ToListAsync(cancellationToken);

        var redeemItems = await _context.RedeemHistories
            .Include(r => r.Customer)
            .Where(r => !r.IsDeleted && r.Status == RedeemStatus.Completed)
            .Where(r => !req.CustomerId.HasValue || r.CustomerId == req.CustomerId)
            .Where(r => !req.DateFrom.HasValue || r.RedeemDate >= req.DateFrom)
            .Where(r => !req.DateTo.HasValue || r.RedeemDate <= req.DateTo)
            .Select(r => new GetPointReportResponse
            {
                Id = r.Id,
                CustomerName = r.Customer.FullName,
                MemberCode = r.Customer.MemberCode,
                ActivityType = "Redeem",
                Points = -r.PointsUsed,
                Description = $"Redeem: {r.RewardNameSnapshot}",
                ActivityDate = r.RedeemDate,
                BalanceAfter = 0
            })
            .ToListAsync(cancellationToken);

        var adjustItems = await _context.PointAdjustments
            .Include(a => a.Customer)
            .Where(a => !a.IsDeleted)
            .Where(a => !req.CustomerId.HasValue || a.CustomerId == req.CustomerId)
            .Where(a => !req.DateFrom.HasValue || a.AdjustmentDate >= req.DateFrom)
            .Where(a => !req.DateTo.HasValue || a.AdjustmentDate <= req.DateTo)
            .Select(a => new GetPointReportResponse
            {
                Id = a.Id,
                CustomerName = a.Customer.FullName,
                MemberCode = a.Customer.MemberCode,
                ActivityType = "Adjustment",
                Points = a.Points,
                Description = a.Reason,
                ActivityDate = a.AdjustmentDate,
                BalanceAfter = 0
            })
            .ToListAsync(cancellationToken);

        var allItems = earnItems.Concat(redeemItems).Concat(adjustItems)
            .Where(i => string.IsNullOrWhiteSpace(req.Search)
                || i.CustomerName.Contains(req.Search, StringComparison.OrdinalIgnoreCase)
                || i.MemberCode.Contains(req.Search, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.ActivityDate)
            .ToList();

        var total = allItems.Count;
        var paged = allItems
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToList();

        return new PaginatedListResponse<GetPointReportResponse> { Items = paged, TotalCount = total };
    }
}
