using Smlp.Base.Reward.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Master.Rewards.Queries;

public class GetRewardResponse : Response
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public RewardType Type { get; set; }
    public int PointCost { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public string? FreeItemName { get; set; }
    public int? StockLimit { get; set; }
    public int RedeemedCount { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public int MinimumTier { get; set; }
    public int StockRemaining => StockLimit.HasValue ? Math.Max(0, StockLimit.Value - RedeemedCount) : int.MaxValue;
}
