using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Application.Transactions.Commands.Cancel;

public class CancelTransactionCommand : IRequest<SuccessResponse>
{
    public Guid Id { get; set; }
}

public class CancelTransactionCommandHandler : IRequestHandler<CancelTransactionCommand, SuccessResponse>
{
    private readonly ISmlpDbContext _context;

    public CancelTransactionCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<SuccessResponse> Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Transaction", request.Id);

        if (transaction.Status == TransactionStatus.Cancelled)
            throw new InvalidOperationException("Transaksi ini sudah dibatalkan sebelumnya.");

        if (transaction.Status != TransactionStatus.Completed)
            throw new InvalidOperationException("Hanya transaksi dengan status Completed yang dapat dibatalkan.");

        // Rollback: kurangi poin dari customer
        var pointsToRollback = transaction.PointsEarned + transaction.BonusPoints;
        var customer = transaction.Customer;

        customer.TotalPoints = Math.Max(0, customer.TotalPoints - pointsToRollback);
        customer.LifetimePoints = Math.Max(0, customer.LifetimePoints - pointsToRollback);
        customer.TotalSpend = Math.Max(0, customer.TotalSpend - transaction.TotalAmount);

        // Recalculate tier
        customer.Tier = customer.LifetimePoints switch
        {
            >= 5000 => Smlp.Base.Customer.Enums.CustomerTier.Platinum,
            >= 2000 => Smlp.Base.Customer.Enums.CustomerTier.Gold,
            >= 500 => Smlp.Base.Customer.Enums.CustomerTier.Silver,
            _ => Smlp.Base.Customer.Enums.CustomerTier.Bronze,
        };

        transaction.Status = TransactionStatus.Cancelled;

        await _context.SaveChangesAsync(this, cancellationToken);

        return new SuccessResponse();
    }
}
