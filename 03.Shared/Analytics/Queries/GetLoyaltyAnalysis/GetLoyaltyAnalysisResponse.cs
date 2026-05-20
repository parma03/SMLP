using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;

public class GetLoyaltyAnalysisResponse : Response
{
    // Point economy
    public long TotalPointsIssued { get; set; }
    public long TotalPointsRedeemed { get; set; }
    public long TotalPointsExpired { get; set; }
    public long TotalPointsCirculating { get; set; }
    public decimal RedemptionRate { get; set; }

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
