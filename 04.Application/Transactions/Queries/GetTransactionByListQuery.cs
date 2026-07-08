using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.Application.Transactions.Queries;

public class GetTransactionByListQuery : IRequest<PaginatedListResponse<GetTransactionResponse>>
{
    public GetTransactionRequest Request { get; set; } = default!;
}

public class GetTransactionByListQueryHandler : IRequestHandler<GetTransactionByListQuery, PaginatedListResponse<GetTransactionResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetTransactionByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetTransactionResponse>> Handle(GetTransactionByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Customer)
            .Include(t => t.Items)
            .Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(t =>
                t.TransactionCode.Contains(req.Search) ||
                t.Customer.FullName.Contains(req.Search) ||
                t.Customer.MemberCode.Contains(req.Search));

        if (req.CustomerId.HasValue)
            query = query.Where(t => t.CustomerId == req.CustomerId);

        if (req.DateFrom.HasValue)
            query = query.Where(t => t.TransactionDate >= req.DateFrom);

        if (req.DateTo.HasValue)
            query = query.Where(t => t.TransactionDate <= req.DateTo);

        if (req.Status.HasValue)
            query = query.Where(t => t.Status == req.Status);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(t => new GetTransactionResponse
            {
                Id = t.Id,
                TransactionCode = t.TransactionCode,
                CustomerId = t.CustomerId,
                CustomerName = t.Customer.FullName,
                CustomerMemberCode = t.Customer.MemberCode,
                TransactionDate = t.TransactionDate,
                TotalAmount = t.TotalAmount,
                PointsEarned = t.PointsEarned,
                BonusPoints = t.BonusPoints,
                PaymentMethod = t.PaymentMethod,
                Notes = t.Notes,
                Status = t.Status,
                CustomerTierSnapshot = t.CustomerTierSnapshot,
                Items = t.Items.Select(i => new TransactionItemResponse
                {
                    Id = i.Id,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.SubTotal,
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetTransactionResponse> { Items = items, TotalCount = total };
    }
}
