namespace Smlp.Shared.Transaction.Transactions.Queries;

public class TransactionItemDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}
