namespace Smlp.Shared.Reports.Queries.GetTransactionReport;

public class GetTransactionReportRequest
{
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
