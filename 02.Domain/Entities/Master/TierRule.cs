using Smlp.Base.Customer.Enums;
using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities.Master
{
    public class TierRule : AuditableEntity
    {
        public string Name { get; set; } = default!;
        public CustomerTier Tier { get; set; }
        public int MinimumLifetimePoints { get; set; }
        public int MaximumLifetimePoints { get; set; }
        public decimal PointMultiplier { get; set; }
        public string? BenefitDescription { get; set; }
        public string? BadgeColor { get; set; }
        public bool IsActive { get; set; }
    }
}
