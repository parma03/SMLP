namespace Smlp.Shared.Analytics.Queries.GetCustomerInsights;

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
