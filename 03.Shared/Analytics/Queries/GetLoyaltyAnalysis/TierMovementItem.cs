namespace Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;

public class TierMovementItem
{
    public string Month { get; set; } = default!;
    public int Bronze { get; set; }
    public int Silver { get; set; }
    public int Gold { get; set; }
    public int Platinum { get; set; }
}
