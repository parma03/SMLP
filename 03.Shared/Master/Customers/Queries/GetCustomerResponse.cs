using Smlp.Base.Customer.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Master.Customers.Queries;

public class GetCustomerResponse : Response
{
    public Guid Id { get; set; }
    public string MemberCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? EmailAddress { get; set; }
    public DateOnly? BirthDate { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public CustomerTier Tier { get; set; }
    public CustomerSegment Segment { get; set; }
    public int TotalPoints { get; set; }
    public int LifetimePoints { get; set; }
    public decimal TotalSpend { get; set; }
    public DateTimeOffset? LastTransactionDate { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
