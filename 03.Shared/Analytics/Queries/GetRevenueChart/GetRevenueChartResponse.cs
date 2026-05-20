using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Analytics.Queries.GetRevenueChart;

public class GetRevenueChartResponse : Response
{
    public List<RevenueDataPoint> MonthlyRevenue { get; set; } = new();
    public List<RevenueDataPoint> DailyRevenue { get; set; } = new();
    public List<PaymentMethodBreakdown> PaymentMethods { get; set; } = new();
}
