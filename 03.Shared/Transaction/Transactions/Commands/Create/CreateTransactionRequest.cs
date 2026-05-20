using Smlp.Base.Transaction.Enums;

namespace Smlp.Shared.Transaction.Transactions.Commands.Create;

public class CreateTransactionRequest
{
    public Guid CustomerId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public List<CreateTransactionItemRequest> Items { get; set; } = new();
}
