namespace Smlp.Shared.Analytics.Queries.GetRevenueChart;

public class RevenueDataPoint
{
    public string Label { get; set; } = default!;
    public decimal Amount { get; set; }
    public int Transactions { get; set; }
    public int PointsEarned { get; set; }
}
