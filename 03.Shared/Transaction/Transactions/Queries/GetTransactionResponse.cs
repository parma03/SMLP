using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Transaction.Transactions.Queries;

public class GetTransactionResponse : Response
{
    public Guid Id { get; set; }
    public string TransactionCode { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerMemberCode { get; set; } = default!;
    public DateTimeOffset TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int PointsEarned { get; set; }
    public int BonusPoints { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public TransactionStatus Status { get; set; }
    public string CustomerTierSnapshot { get; set; } = default!;
    public List<TransactionItemDto> Items { get; set; } = new();
}
