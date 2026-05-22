namespace Smlp.Shared.Master.TierRules.Queries;

public class GetTierRuleRequest
{
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
