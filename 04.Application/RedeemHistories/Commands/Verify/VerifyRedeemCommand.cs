using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Caching;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Caching;
using Smlp.Application.Services.CurrentUser;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Services.Monitoring;
using Smlp.Shared.Transaction.RedeemHistories.Commands.Verify;

namespace Smlp.Application.RedeemHistories.Commands.Verify;

public class VerifyRedeemCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public VerifyRedeemRequest Request { get; set; } = default!;
}

public class VerifyRedeemCommandHandler : IRequestHandler<VerifyRedeemCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public VerifyRedeemCommandHandler(ISmlpDbContext context, ICurrentUserService currentUser, ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<CommonUpdateResponse> Handle(VerifyRedeemCommand request, CancellationToken cancellationToken)
    {
        var redeem = await _context.RedeemHistories
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("RedeemHistory", request.Id);

        if (redeem.Status != RedeemStatus.Pending)
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    "Status",
                    $"Redeem sudah dalam status {redeem.Status}, tidak dapat diverifikasi ulang."
                )
            });

        var req = request.Request;

        // If rejected, refund points to customer
        if (req.Status == RedeemStatus.Rejected)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == redeem.CustomerId && !c.IsDeleted, cancellationToken);

            if (customer != null)
            {
                customer.TotalPoints += redeem.PointsUsed;

                // Decrement reward redeemed count
                var reward = await _context.Rewards
                    .FirstOrDefaultAsync(r => r.Id == redeem.RewardId, cancellationToken);
                if (reward != null)
                    reward.RedeemedCount = Math.Max(0, reward.RedeemedCount - 1);
            }
        }

        redeem.Status = req.Status;
        redeem.VerifiedBy = _currentUser.Username;
        redeem.VerifiedAt = DateTimeOffset.UtcNow;
        redeem.Notes = req.Notes ?? redeem.Notes;

        await _context.SaveChangesAsync(this, cancellationToken);

        // Verifikasi redeem mengubah status pending & (bila ditolak) poin yang dikembalikan ke customer
        await _cache.RemoveAsync(CacheKeys.DashboardSummary, cancellationToken);

        // Business metric untuk monitoring (Prometheus/Grafana/App Insights)
        SmlpMetrics.RedeemsVerified.Add(1, new KeyValuePair<string, object?>("status", redeem.Status.ToString()));

        return new CommonUpdateResponse { Id = redeem.Id };
    }
}
