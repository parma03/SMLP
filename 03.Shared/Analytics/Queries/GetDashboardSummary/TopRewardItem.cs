namespace Smlp.Shared.Analytics.Queries.GetDashboardSummary;

public class TopRewardItem
{
    public string RewardName { get; set; } = default!;
    public int RedeemCount { get; set; }
    public int PointCost { get; set; }
}
