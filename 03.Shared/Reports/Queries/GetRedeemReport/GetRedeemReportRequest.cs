namespace Smlp.Shared.Reports.Queries.GetRedeemReport;

public class GetRedeemReportRequest
{
    public Guid? CustomerId { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
