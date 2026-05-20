namespace Smlp.Shared.Analytics.Queries.GetDashboardSummary;

public class GetDashboardSummaryResponse
{
    // KPI Cards
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }

    public decimal TotalRevenueThisMonth { get; set; }
    public decimal TotalRevenueLastMonth { get; set; }
    public decimal RevenueGrowthPercent { get; set; }

    public int TotalTransactionsToday { get; set; }
    public int TotalTransactionsThisMonth { get; set; }

    public int TotalPointsCirculating { get; set; }
    public int TotalPointsRedeemed { get; set; }
    public int TotalPendingRedeems { get; set; }

    // Tier breakdown
    public int BronzeCount { get; set; }
    public int SilverCount { get; set; }
    public int GoldCount { get; set; }
    public int PlatinumCount { get; set; }

    // Top rewards
    public List<TopRewardItem> TopRewards { get; set; } = new();
}

public class TopRewardItem
{
    public string RewardName { get; set; } = default!;
    public int RedeemCount { get; set; }
    public int PointCost { get; set; }
}
