namespace Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;

public class GetLoyaltyAnalysisResponse
{
    // Point economy
    public long TotalPointsIssued { get; set; }
    public long TotalPointsRedeemed { get; set; }
    public long TotalPointsExpired { get; set; }
    public long TotalPointsCirculating { get; set; }
    public decimal RedemptionRate { get; set; }  // redeemed / issued %

    // Reward performance
    public List<RewardPerformanceItem> RewardPerformance { get; set; } = new();

    // Tier upgrade trend (monthly)
    public List<TierMovementItem> TierMovements { get; set; } = new();

    // Redemption trend (monthly)
    public List<RedemptionTrendItem> RedemptionTrend { get; set; } = new();

    // Point adjustment summary
    public int TotalManualAdditions { get; set; }
    public int TotalManualDeductions { get; set; }
    public int TotalPointsManuallyAdded { get; set; }
    public int TotalPointsManuallyDeducted { get; set; }
}

public class RewardPerformanceItem
{
    public Guid RewardId { get; set; }
    public string RewardName { get; set; } = default!;
    public int RedeemCount { get; set; }
    public int TotalPointsUsed { get; set; }
    public int? StockLimit { get; set; }
    public int StockUsed { get; set; }
}

public class TierMovementItem
{
    public string Month { get; set; } = default!;
    public int Bronze { get; set; }
    public int Silver { get; set; }
    public int Gold { get; set; }
    public int Platinum { get; set; }
}

public class RedemptionTrendItem
{
    public string Month { get; set; } = default!;
    public int Count { get; set; }
    public int TotalPoints { get; set; }
}
