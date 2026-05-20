namespace Smlp.Shared.Analytics.Queries.GetCustomerInsights;

public class GetCustomerInsightsResponse
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

public class TopCustomerItem
{
    public Guid Id { get; set; }
    public string MemberCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Tier { get; set; } = default!;
    public decimal TotalSpend { get; set; }
    public int TotalPoints { get; set; }
    public int TransactionCount { get; set; }
}

public class BirthdayCustomerItem
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string MemberCode { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int TotalPoints { get; set; }
}

public class ChurnRiskItem
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string MemberCode { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string Tier { get; set; } = default!;
    public int DaysSinceLastPurchase { get; set; }
    public int TotalPoints { get; set; }
}
