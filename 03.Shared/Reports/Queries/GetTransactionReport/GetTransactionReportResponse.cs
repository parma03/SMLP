using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Reports.Queries.GetTransactionReport;

public class GetTransactionReportResponse : Response
{
    public Guid Id { get; set; }
    public string TransactionCode { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string MemberCode { get; set; } = default!;
    public DateTimeOffset TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int PointsEarned { get; set; }
    public string PaymentMethod { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Tier { get; set; } = default!;
}
