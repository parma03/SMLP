using Smlp.Base.Reward.Enums;

namespace Smlp.Shared.Master.Rewards.Commands.Update;

public class UpdateRewardRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public RewardType Type { get; set; }
    public int PointCost { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public string? FreeItemName { get; set; }
    public int? StockLimit { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public int MinimumTier { get; set; }
}
