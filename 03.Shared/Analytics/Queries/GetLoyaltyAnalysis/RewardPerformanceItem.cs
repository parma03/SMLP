namespace Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;

public class RewardPerformanceItem
{
    public Guid RewardId { get; set; }
    public string RewardName { get; set; } = default!;
    public int RedeemCount { get; set; }
    public int TotalPointsUsed { get; set; }
    public int? StockLimit { get; set; }
    public int StockUsed { get; set; }
}
