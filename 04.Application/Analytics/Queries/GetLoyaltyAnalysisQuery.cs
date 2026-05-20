using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;

namespace Smlp.Application.Analytics.Queries;

public class GetLoyaltyAnalysisQuery : IRequest<GetLoyaltyAnalysisResponse> { }

public class GetLoyaltyAnalysisQueryHandler : IRequestHandler<GetLoyaltyAnalysisQuery, GetLoyaltyAnalysisResponse>
{
    private readonly ISmlpDbContext _context;

    public GetLoyaltyAnalysisQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetLoyaltyAnalysisResponse> Handle(GetLoyaltyAnalysisQuery request, CancellationToken cancellationToken)
    {
        // Point economy
        var totalPointsIssued = await _context.Transactions
            .Where(t => !t.IsDeleted && t.Status == TransactionStatus.Completed)
            .SumAsync(t => (long?)(t.PointsEarned + t.BonusPoints), cancellationToken) ?? 0;

        var totalPointsRedeemed = await _context.RedeemHistories
            .Where(r => !r.IsDeleted && r.Status == RedeemStatus.Completed)
            .SumAsync(r => (long?)r.PointsUsed, cancellationToken) ?? 0;

        var totalPointsManualAdded = await _context.PointAdjustments
            .Where(a => !a.IsDeleted && a.Points > 0)
            .SumAsync(a => (long?)a.Points, cancellationToken) ?? 0;

        var totalPointsManualDeducted = await _context.PointAdjustments
            .Where(a => !a.IsDeleted && a.Points < 0)
            .SumAsync(a => (long?)-a.Points, cancellationToken) ?? 0;

        var totalCirculating = totalPointsIssued + totalPointsManualAdded - totalPointsRedeemed - totalPointsManualDeducted;
        var redemptionRate = totalPointsIssued > 0
            ? Math.Round((decimal)totalPointsRedeemed / totalPointsIssued * 100, 2)
            : 0;

        // Reward performance
        var rewardPerformance = await _context.RedeemHistories
            .Where(r => !r.IsDeleted)
            .GroupBy(r => new { r.RewardId, r.RewardNameSnapshot })
            .Select(g => new RewardPerformanceItem
            {
                RewardId = g.Key.RewardId,
                RewardName = g.Key.RewardNameSnapshot,
                RedeemCount = g.Count(),
                TotalPointsUsed = g.Sum(r => r.PointsUsed),
                StockUsed = g.Count(r => r.Status == RedeemStatus.Completed)
            })
            .OrderByDescending(r => r.RedeemCount)
            .ToListAsync(cancellationToken);

        // Redemption trend — last 6 months
        var sixMonthsAgo = DateTimeOffset.UtcNow.AddMonths(-6);
        var redeemsByMonth = await _context.RedeemHistories
            .Where(r => !r.IsDeleted && r.Status == RedeemStatus.Completed && r.RedeemDate >= sixMonthsAgo)
            .GroupBy(r => new { r.RedeemDate.Year, r.RedeemDate.Month })
            .Select(g => new RedemptionTrendItem
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Count = g.Count(),
                TotalPoints = g.Sum(r => r.PointsUsed)
            })
            .OrderBy(r => r.Month)
            .ToListAsync(cancellationToken);

        // Tier movement (monthly snapshot from customer current tiers grouped by month joined)
        var tierMovements = await _context.Customers
            .Where(c => !c.IsDeleted && c.Created >= sixMonthsAgo)
            .GroupBy(c => new { c.Created.Year, c.Created.Month, c.Tier })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Tier, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var tierMovementResult = tierMovements
            .GroupBy(x => $"{x.Year}-{x.Month:D2}")
            .Select(g => new TierMovementItem
            {
                Month = g.Key,
                Bronze = g.Where(x => x.Tier == Smlp.Base.Customer.Enums.CustomerTier.Bronze).Sum(x => x.Count),
                Silver = g.Where(x => x.Tier == Smlp.Base.Customer.Enums.CustomerTier.Silver).Sum(x => x.Count),
                Gold = g.Where(x => x.Tier == Smlp.Base.Customer.Enums.CustomerTier.Gold).Sum(x => x.Count),
                Platinum = g.Where(x => x.Tier == Smlp.Base.Customer.Enums.CustomerTier.Platinum).Sum(x => x.Count),
            })
            .OrderBy(x => x.Month)
            .ToList();

        // Manual adjustments summary
        var manualAddCount = await _context.PointAdjustments.CountAsync(a => !a.IsDeleted && a.Points > 0, cancellationToken);
        var manualDeductCount = await _context.PointAdjustments.CountAsync(a => !a.IsDeleted && a.Points < 0, cancellationToken);

        return new GetLoyaltyAnalysisResponse
        {
            TotalPointsIssued = totalPointsIssued,
            TotalPointsRedeemed = totalPointsRedeemed,
            TotalPointsExpired = 0,
            TotalPointsCirculating = totalCirculating,
            RedemptionRate = redemptionRate,
            RewardPerformance = rewardPerformance,
            TierMovements = tierMovementResult,
            RedemptionTrend = redeemsByMonth,
            TotalManualAdditions = manualAddCount,
            TotalManualDeductions = manualDeductCount,
            TotalPointsManuallyAdded = (int)totalPointsManualAdded,
            TotalPointsManuallyDeducted = (int)totalPointsManualDeducted
        };
    }
}
