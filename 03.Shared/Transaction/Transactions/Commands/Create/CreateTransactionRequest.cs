using Smlp.Base.Transaction.Enums;

namespace Smlp.Shared.Transaction.Transactions.Commands.Create;

public class CreateTransactionRequest
{
    public Guid CustomerId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public List<CreateTransactionItemRequest> Items { get; set; } = new();
}

public class CreateTransactionItemRequest
{
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
}
