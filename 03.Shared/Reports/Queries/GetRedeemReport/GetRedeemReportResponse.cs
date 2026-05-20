using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Reports.Queries.GetRedeemReport;

public class GetRedeemReportResponse : Response
{
    public Guid Id { get; set; }
    public string RedeemCode { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string MemberCode { get; set; } = default!;
    public string RewardName { get; set; } = default!;
    public DateTimeOffset RedeemDate { get; set; }
    public int PointsUsed { get; set; }
    public string Status { get; set; } = default!;
    public string? VerifiedBy { get; set; }
}
