using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.Application.Transactions.Queries;

public class GetTransactionByIdQuery : IRequest<GetTransactionResponse>
{
    public Guid Id { get; set; }
}

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, GetTransactionResponse>
{
    private readonly ISmlpDbContext _context;

    public GetTransactionByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetTransactionResponse> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await _context.Transactions
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Transaction", request.Id);

        return new GetTransactionResponse
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
            Items = t.Items.Select(i => new TransactionItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.SubTotal,
            }).ToList()
        };
    }
}
