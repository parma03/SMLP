using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Reports.Queries.GetTransactionReport;

namespace Smlp.Application.Reports.Queries;

public class GetTransactionReportQuery : IRequest<PaginatedListResponse<GetTransactionReportResponse>>
{
    public GetTransactionReportRequest Request { get; set; } = default!;
}

public class GetTransactionReportQueryHandler : IRequestHandler<GetTransactionReportQuery, PaginatedListResponse<GetTransactionReportResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetTransactionReportQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetTransactionReportResponse>> Handle(GetTransactionReportQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.Transactions
            .Include(t => t.Customer)
            .Where(t => !t.IsDeleted && t.Status == TransactionStatus.Completed);

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(t => t.TransactionCode.Contains(req.Search)
                || t.Customer.FullName.Contains(req.Search)
                || t.Customer.MemberCode.Contains(req.Search));

        if (req.CustomerId.HasValue)
            query = query.Where(t => t.CustomerId == req.CustomerId);

        if (req.DateFrom.HasValue)
            query = query.Where(t => t.TransactionDate >= req.DateFrom);

        if (req.DateTo.HasValue)
            query = query.Where(t => t.TransactionDate <= req.DateTo);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(t => new GetTransactionReportResponse
            {
                Id = t.Id,
                TransactionCode = t.TransactionCode,
                CustomerName = t.Customer.FullName,
                MemberCode = t.Customer.MemberCode,
                TransactionDate = t.TransactionDate,
                TotalAmount = t.TotalAmount,
                PointsEarned = t.PointsEarned,
                PaymentMethod = t.PaymentMethod.ToString(),
                Status = t.Status.ToString(),
                Tier = t.CustomerTierSnapshot
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetTransactionReportResponse> { Items = items, TotalCount = total };
    }
}
