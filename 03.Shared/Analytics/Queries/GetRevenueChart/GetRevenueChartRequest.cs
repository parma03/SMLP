namespace Smlp.Shared.Analytics.Queries.GetRevenueChart;

public class GetRevenueChartRequest
{
    public int Year { get; set; } = DateTime.Now.Year;
    public int Month { get; set; } = 0; // 0 = full year
}
