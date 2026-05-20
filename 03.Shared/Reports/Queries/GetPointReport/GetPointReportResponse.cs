using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Reports.Queries.GetPointReport;

public class GetPointReportResponse : Response
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = default!;
    public string MemberCode { get; set; } = default!;
    public string ActivityType { get; set; } = default!; // Earn / Redeem / Adjustment
    public int Points { get; set; }
    public string Description { get; set; } = default!;
    public DateTimeOffset ActivityDate { get; set; }
    public int BalanceAfter { get; set; }
}
