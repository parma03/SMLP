namespace Smlp.Shared.Analytics.Queries.GetCustomerInsights;

public class BirthdayCustomerItem
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string MemberCode { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int TotalPoints { get; set; }
}
