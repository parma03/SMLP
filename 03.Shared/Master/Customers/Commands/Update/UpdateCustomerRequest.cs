using Smlp.Base.Customer.Enums;

namespace Smlp.Shared.Master.Customers.Commands.Update;

public class UpdateCustomerRequest
{
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? EmailAddress { get; set; }
    public DateOnly? BirthDate { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}
