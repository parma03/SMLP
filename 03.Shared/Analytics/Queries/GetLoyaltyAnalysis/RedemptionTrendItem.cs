namespace Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;

public class RedemptionTrendItem
{
    public string Month { get; set; } = default!;
    public int Count { get; set; }
    public int TotalPoints { get; set; }
}
