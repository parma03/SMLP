using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Caching;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Caching;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Entities.Transaction;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.RedeemHistories.Commands.Create;

namespace Smlp.Application.RedeemHistories.Commands.Create;

public class CreateRedeemCommand : IRequest<CommonCreateResponse>
{
    public CreateRedeemRequest Request { get; set; } = default!;
}

public class CreateRedeemCommandHandler : IRequestHandler<CreateRedeemCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly ICacheService _cache;

    public CreateRedeemCommandHandler(ISmlpDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CommonCreateResponse> Handle(CreateRedeemCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == req.CustomerId && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Customer", req.CustomerId);

        var reward = await _context.Rewards
            .FirstOrDefaultAsync(r => r.Id == req.RewardId && !r.IsDeleted && r.IsActive, cancellationToken)
            ?? throw new NotFoundException("Reward", req.RewardId);

        if (customer.TotalPoints < reward.PointCost)
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    "Points",
                    $"Poin tidak mencukupi. Dibutuhkan {reward.PointCost} poin, saldo: {customer.TotalPoints} poin."
                )
            });

        if (reward.StockLimit.HasValue && reward.RedeemedCount >= reward.StockLimit.Value)
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    "Stock",
                    "Stok reward sudah habis."
                )
            });

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (reward.ValidFrom.HasValue && reward.ValidFrom > today)
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    "ValidFrom",
                    $"Reward belum berlaku hingga {reward.ValidFrom}."
                )
            });

        if (reward.ValidUntil.HasValue && reward.ValidUntil < today)
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    "ValidUntil",
                    "Reward sudah kedaluwarsa."
                )
            });

        var redeemCode = await GenerateRedeemCodeAsync(cancellationToken);

        var redeem = new RedeemHistory
        {
            RedeemCode = redeemCode,
            CustomerId = customer.Id,
            RewardId = reward.Id,
            RedeemDate = DateTimeOffset.UtcNow,
            PointsUsed = reward.PointCost,
            Status = RedeemStatus.Pending,
            RewardNameSnapshot = reward.Name,
            PointCostSnapshot = reward.PointCost,
            Notes = req.Notes,
        };

        await _context.RedeemHistories.AddAsync(redeem, cancellationToken);

        // Deduct points from customer
        customer.TotalPoints -= reward.PointCost;

        // Increment reward redeemed count
        reward.RedeemedCount += 1;

        await _context.SaveChangesAsync(this, cancellationToken);

        // Redeem baru mengubah jumlah pending redeem & poin beredar di dashboard
        await _cache.RemoveAsync(CacheKeys.DashboardSummary, cancellationToken);

        return new CommonCreateResponse { Id = redeem.Id };
    }

    private async Task<string> GenerateRedeemCodeAsync(CancellationToken ct)
    {
        var count = await _context.RedeemHistories.CountAsync(ct);
        var date = DateTime.Today.ToString("yyyyMMdd");
        return $"RDM-{date}-{count + 1:D5}";
    }
}
