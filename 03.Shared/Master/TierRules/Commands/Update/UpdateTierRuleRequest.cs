using Smlp.Base.Customer.Enums;

namespace Smlp.Shared.Master.TierRules.Commands.Update;

public class UpdateTierRuleRequest
{
    public string Name { get; set; } = default!;
    public CustomerTier Tier { get; set; }
    public int MinimumLifetimePoints { get; set; }
    public int MaximumLifetimePoints { get; set; }
    public decimal PointMultiplier { get; set; } = 1.0m;
    public string? BenefitDescription { get; set; }
    public string? BadgeColor { get; set; }
    public bool IsActive { get; set; }
}
