using Smlp.Base.Customer.Enums;

namespace Smlp.Shared.Master.Customers.Queries;

public class GetCustomerRequest
{
    public string? Search { get; set; }
    public CustomerTier? Tier { get; set; }
    public CustomerSegment? Segment { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
