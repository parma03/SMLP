using Smlp.Base.Transaction.Enums;

namespace Smlp.Shared.Transaction.Transactions.Queries;

public class GetTransactionRequest
{
    public string? Search { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public TransactionStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
