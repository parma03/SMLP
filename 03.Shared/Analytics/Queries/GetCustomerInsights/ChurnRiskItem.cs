namespace Smlp.Shared.Analytics.Queries.GetCustomerInsights;

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
