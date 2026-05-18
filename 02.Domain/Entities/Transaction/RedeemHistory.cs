using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Abstracts;
using Smlp.Domain.Entities.Master;

namespace Smlp.Domain.Entities.Transaction
{
    public class RedeemHistory : AuditableEntity
    {
        public string RedeemCode { get; set; } = default!;
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        public Guid RewardId { get; set; }
        public Reward Reward { get; set; } = default!;
        public DateTimeOffset RedeemDate { get; set; }
        public int PointsUsed { get; set; }
        public RedeemStatus Status { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public string? Notes { get; set; }
        public string RewardNameSnapshot { get; set; } = default!;
        public int PointCostSnapshot { get; set; }
    }
}
