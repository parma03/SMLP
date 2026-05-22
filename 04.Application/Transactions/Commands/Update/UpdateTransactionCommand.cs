using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Entities.Transaction;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.Transactions.Commands.Update;

namespace Smlp.Application.Transactions.Commands.Update;

public class UpdateTransactionCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdateTransactionRequest Request { get; set; } = default!;
}

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;

    public UpdateTransactionCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonUpdateResponse> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var transaction = await _context.Transactions
            .Include(t => t.Items)
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Transaction", request.Id);

        if (transaction.Status == TransactionStatus.Cancelled)
            throw new InvalidOperationException("Transaksi yang sudah dibatalkan tidak dapat diubah.");

        // Recalculate totals
        var subtotal = req.Items.Sum(i => i.UnitPrice * i.Quantity);
        var discount = Math.Min(req.DiscountAmount, subtotal);
        var newTotal = Math.Max(0, subtotal - discount);

        // Recalculate points
        var oldPoints = transaction.PointsEarned + transaction.BonusPoints;

        // Get active point rule
        var today = DateOnly.FromDateTime(DateTime.Today);
        var pointRule = await _context.PointRules
            .Where(r => r.IsActive && !r.IsDeleted
                && (r.ValidFrom == null || r.ValidFrom <= today)
                && (r.ValidUntil == null || r.ValidUntil >= today))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var newPoints = 0;
        if (pointRule != null && pointRule.SpendAmount > 0)
        {
            var multiplier = pointRule.BonusMultiplier ?? 1m;
            newPoints = (int)(newTotal / pointRule.SpendAmount * pointRule.PointEarned * multiplier);
        }

        // Adjust customer points delta
        var customer = transaction.Customer;
        var pointDelta = newPoints - oldPoints;
        customer.TotalPoints = Math.Max(0, customer.TotalPoints + pointDelta);
        customer.LifetimePoints = Math.Max(0, customer.LifetimePoints + pointDelta);
        customer.TotalSpend = Math.Max(0, customer.TotalSpend - transaction.TotalAmount + newTotal);

        // Update transaction
        transaction.PaymentMethod = req.PaymentMethod;
        transaction.Notes = req.Notes;
        transaction.TotalAmount = newTotal;
        transaction.PointsEarned = newPoints;

        // Replace items
        _context.TransactionItems.RemoveRange(transaction.Items);

        foreach (var item in req.Items.Where(i => !string.IsNullOrWhiteSpace(i.ProductName) && i.UnitPrice > 0 && i.Quantity > 0))
        {
            _context.TransactionItems.Add(new TransactionItem
            {
                TransactionId = transaction.Id,
                ProductName = item.ProductName!,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.UnitPrice * item.Quantity,
            });
        }

        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonUpdateResponse { Id = transaction.Id };
    }
}
