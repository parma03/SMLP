using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Transaction.RedeemHistories.Queries;

public class GetRedeemHistoryResponse : Response
{
    public Guid Id { get; set; }
    public string RedeemCode { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerMemberCode { get; set; } = default!;
    public Guid RewardId { get; set; }
    public string RewardName { get; set; } = default!;
    public DateTimeOffset RedeemDate { get; set; }
    public int PointsUsed { get; set; }
    public RedeemStatus Status { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? Notes { get; set; }
    public int PointCostSnapshot { get; set; }
}
