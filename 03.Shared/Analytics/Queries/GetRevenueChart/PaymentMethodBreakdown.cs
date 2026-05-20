namespace Smlp.Shared.Analytics.Queries.GetRevenueChart;

public class PaymentMethodBreakdown
{
    public string Method { get; set; } = default!;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}
