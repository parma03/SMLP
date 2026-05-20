namespace Smlp.Shared.Analytics.Queries.GetRevenueChart;

public class GetRevenueChartResponse
{
    public List<RevenueDataPoint> MonthlyRevenue { get; set; } = new();
    public List<RevenueDataPoint> DailyRevenue { get; set; } = new();
    public List<PaymentMethodBreakdown> PaymentMethods { get; set; } = new();
}

public class RevenueDataPoint
{
    public string Label { get; set; } = default!;
    public decimal Amount { get; set; }
    public int Transactions { get; set; }
    public int PointsEarned { get; set; }
}

public class PaymentMethodBreakdown
{
    public string Method { get; set; } = default!;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}
