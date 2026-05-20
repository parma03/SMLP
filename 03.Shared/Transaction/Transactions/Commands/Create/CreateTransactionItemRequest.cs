namespace Smlp.Shared.Transaction.Transactions.Commands.Create;

public class CreateTransactionItemRequest
{
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
}
