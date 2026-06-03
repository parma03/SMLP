using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Analytics.Queries.GetCustomerInsights;

public class GetCustomerInsightsResponse : Response
{
    // Segmentation counts
    public int NewCount { get; set; }
    public int ActiveCount { get; set; }
    public int LoyalCount { get; set; }
    public int AtRiskCount { get; set; }
    public int InactiveCount { get; set; }
    public int TotalCustomers { get; set; }

    // Cohort summary
    public decimal AvgTransactionValue { get; set; }
    public decimal AvgPointsPerCustomer { get; set; }
    public decimal RetentionRate { get; set; }

    // Top spenders
    public List<TopCustomerItem> TopSpenders { get; set; } = new();

    // Birthday this month (CRM)
    public List<BirthdayCustomerItem> BirthdaysThisMonth { get; set; } = new();

    // Churn risk (no purchase > 45 days)
    public List<ChurnRiskItem> ChurnRiskCustomers { get; set; } = new();

    // RFM Segments breakdown (untuk donut chart)
    public List<RfmSegmentItem> RfmSegments { get; set; } = new();
}
