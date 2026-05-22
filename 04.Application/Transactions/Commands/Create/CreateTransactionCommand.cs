using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Customer.Enums;
using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Entities.Transaction;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.Transactions.Commands.Create;

namespace Smlp.Application.Transactions.Commands.Create;

public class CreateTransactionCommand : IRequest<CommonCreateResponse>
{
    public CreateTransactionRequest Request { get; set; } = default!;
}

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;

    public CreateTransactionCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonCreateResponse> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == req.CustomerId && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Customer", req.CustomerId);

        // Get active point rule
        var today = DateOnly.FromDateTime(DateTime.Today);
        var pointRule = await _context.PointRules
            .Where(r => r.IsActive && !r.IsDeleted
                && (r.ValidFrom == null || r.ValidFrom <= today)
                && (r.ValidUntil == null || r.ValidUntil >= today))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var txCode = await GenerateTransactionCodeAsync(cancellationToken);
        var totalAmount = req.Items.Sum(i => i.UnitPrice * i.Quantity);

        var pointsEarned = 0;
        var multiplier = 1m;

        if (pointRule != null && pointRule.SpendAmount > 0)
        {
            multiplier = pointRule.BonusMultiplier ?? 1m;
            pointsEarned = (int)(totalAmount / pointRule.SpendAmount * pointRule.PointEarned * multiplier);
        }

        var transaction = new Transaksi
        {
            TransactionCode = txCode,
            CustomerId = customer.Id,
            TransactionDate = DateTimeOffset.UtcNow,
            TotalAmount = totalAmount,
            PointsEarned = pointsEarned,
            BonusPoints = 0,
            PaymentMethod = req.PaymentMethod,
            Notes = req.Notes,
            Status = TransactionStatus.Completed,
            CustomerTierSnapshot = customer.Tier.ToString(),
            PointMultiplierSnapshot = multiplier,
            Items = req.Items.Select(i => new TransactionItem
            {
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                SubTotal = i.UnitPrice * i.Quantity,
            }).ToList()
        };

        await _context.Transactions.AddAsync(transaction, cancellationToken);

        // Update customer points and spend
        customer.TotalPoints += pointsEarned;
        customer.LifetimePoints += pointsEarned;
        customer.TotalSpend += totalAmount;
        customer.LastTransactionDate = transaction.TransactionDate;

        // Tier upgrade automation: use TierRules from DB, fall back to hardcoded if none configured
        customer.Tier = await DetermineNewTierAsync(customer.LifetimePoints, cancellationToken);

        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonCreateResponse { Id = transaction.Id };
    }

    /// <summary>
    /// Determines the customer tier based on lifetime points using configured TierRules.
    /// Falls back to hardcoded defaults if no TierRules are configured.
    /// </summary>
    private async Task<CustomerTier> DetermineNewTierAsync(int lifetimePoints, CancellationToken ct)
    {
        var tierRules = await _context.TierRules
            .Where(r => r.IsActive && !r.IsDeleted)
            .OrderByDescending(r => r.MinimumLifetimePoints)
            .ToListAsync(ct);

        if (tierRules.Any())
        {
            // Find the highest tier the customer qualifies for
            foreach (var rule in tierRules)
            {
                var maxOk = rule.MaximumLifetimePoints == 0 || lifetimePoints <= rule.MaximumLifetimePoints;
                if (lifetimePoints >= rule.MinimumLifetimePoints && maxOk)
                {
                    return rule.Tier;
                }
            }
            // If no rule matched, return the lowest tier
            return tierRules.OrderBy(r => r.MinimumLifetimePoints).First().Tier;
        }

        // Fallback to hardcoded defaults
        return lifetimePoints switch
        {
            >= 15000 => CustomerTier.Platinum,
            >= 5000 => CustomerTier.Gold,
            >= 1000 => CustomerTier.Silver,
            _ => CustomerTier.Bronze,
        };
    }

    private async Task<string> GenerateTransactionCodeAsync(CancellationToken ct)
    {
        var count = await _context.Transactions.CountAsync(ct);
        var date = DateTime.Today.ToString("yyyyMMdd");
        return $"TRX-{date}-{count + 1:D5}";
    }
}
