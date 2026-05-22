using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Transaction.Transactions.Commands.Create;

namespace Smlp.Shared.Transaction.Transactions.Commands.Update;

public class UpdateTransactionRequest
{
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public List<CreateTransactionItemRequest> Items { get; set; } = new();
}
