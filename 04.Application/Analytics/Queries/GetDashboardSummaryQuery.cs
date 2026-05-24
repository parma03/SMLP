using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Customer.Enums;
using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Analytics.Queries.GetDashboardSummary;

namespace Smlp.Application.Analytics.Queries;

public class GetDashboardSummaryQuery : IRequest<GetDashboardSummaryResponse> { }

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, GetDashboardSummaryResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "dashboard:summary";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

    public GetDashboardSummaryQueryHandler(ISmlpDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<GetDashboardSummaryResponse> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // Return cached result if available
        if (_cache.TryGetValue(CacheKey, out GetDashboardSummaryResponse? cached) && cached is not null)
            return cached;

        var result = await FetchFromDatabaseAsync(cancellationToken);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiry,
            SlidingExpiration = null,
            Priority = CacheItemPriority.Normal
        };
        _cache.Set(CacheKey, result, cacheOptions);

        return result;
    }

    private async Task<GetDashboardSummaryResponse> FetchFromDatabaseAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var startOfLastMonth = startOfMonth.AddMonths(-1);
        var today = now.Date;

        var totalCustomers = await _context.Customers.CountAsync(c => !c.IsDeleted, cancellationToken);
        var newThisMonth = await _context.Customers.CountAsync(c => !c.IsDeleted && c.CreatedAt >= startOfMonth, cancellationToken);

        var cutoffActive = now.AddDays(-30);
        var activeCustomers = await _context.Customers
            .CountAsync(c => !c.IsDeleted && c.LastTransactionDate >= cutoffActive, cancellationToken);

        var revenueThisMonth = await _context.Transactions
            .Where(t => !t.IsDeleted && t.Status == TransactionStatus.Completed && t.TransactionDate >= startOfMonth)
            .SumAsync(t => (decimal?)t.TotalAmount, cancellationToken) ?? 0;

        var revenueLastMonth = await _context.Transactions
            .Where(t => !t.IsDeleted && t.Status == TransactionStatus.Completed
                     && t.TransactionDate >= startOfLastMonth && t.TransactionDate < startOfMonth)
            .SumAsync(t => (decimal?)t.TotalAmount, cancellationToken) ?? 0;

        var revenueGrowth = revenueLastMonth > 0
            ? Math.Round((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100, 2)
            : 0;

        var txToday = await _context.Transactions
            .CountAsync(t => !t.IsDeleted && t.TransactionDate.Date == today, cancellationToken);

        var txThisMonth = await _context.Transactions
            .CountAsync(t => !t.IsDeleted && t.TransactionDate >= startOfMonth, cancellationToken);

        var totalPointsCirculating = await _context.Customers
            .Where(c => !c.IsDeleted)
            .SumAsync(c => (int?)c.TotalPoints, cancellationToken) ?? 0;

        var totalPointsRedeemed = await _context.RedeemHistories
            .Where(r => !r.IsDeleted && r.Status == RedeemStatus.Completed)
            .SumAsync(r => (int?)r.PointsUsed, cancellationToken) ?? 0;

        var pendingRedeems = await _context.RedeemHistories
            .CountAsync(r => !r.IsDeleted && r.Status == RedeemStatus.Pending, cancellationToken);

        var tierCounts = await _context.Customers
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.Tier)
            .Select(g => new { Tier = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var topRewards = await _context.RedeemHistories
            .Where(r => !r.IsDeleted && r.Status == RedeemStatus.Completed)
            .GroupBy(r => new { r.RewardId, r.RewardNameSnapshot, r.PointCostSnapshot })
            .Select(g => new TopRewardItem
            {
                RewardName = g.Key.RewardNameSnapshot,
                RedeemCount = g.Count(),
                PointCost = g.Key.PointCostSnapshot
            })
            .OrderByDescending(x => x.RedeemCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new GetDashboardSummaryResponse
        {
            TotalCustomers = totalCustomers,
            NewCustomersThisMonth = newThisMonth,
            ActiveCustomers = activeCustomers,
            InactiveCustomers = totalCustomers - activeCustomers,
            TotalRevenueThisMonth = revenueThisMonth,
            TotalRevenueLastMonth = revenueLastMonth,
            RevenueGrowthPercent = revenueGrowth,
            TotalTransactionsToday = txToday,
            TotalTransactionsThisMonth = txThisMonth,
            TotalPointsCirculating = totalPointsCirculating,
            TotalPointsRedeemed = totalPointsRedeemed,
            TotalPendingRedeems = pendingRedeems,
            BronzeCount = tierCounts.FirstOrDefault(t => t.Tier == CustomerTier.Bronze)?.Count ?? 0,
            SilverCount = tierCounts.FirstOrDefault(t => t.Tier == CustomerTier.Silver)?.Count ?? 0,
            GoldCount = tierCounts.FirstOrDefault(t => t.Tier == CustomerTier.Gold)?.Count ?? 0,
            PlatinumCount = tierCounts.FirstOrDefault(t => t.Tier == CustomerTier.Platinum)?.Count ?? 0,
            TopRewards = topRewards
        };
    }
}
