using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Analytics.Queries.GetCustomerInsights;

public class GetCustomerInsightsResponse : Response
{
    // Segmentation
    public int NewCount { get; set; }
    public int ActiveCount { get; set; }
    public int LoyalCount { get; set; }
    public int AtRiskCount { get; set; }
    public int InactiveCount { get; set; }

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
}
