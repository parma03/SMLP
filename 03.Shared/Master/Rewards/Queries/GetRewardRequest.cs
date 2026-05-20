using Smlp.Base.Reward.Enums;

namespace Smlp.Shared.Master.Rewards.Queries;

public class GetRewardRequest
{
    public string? Search { get; set; }
    public RewardType? Type { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
