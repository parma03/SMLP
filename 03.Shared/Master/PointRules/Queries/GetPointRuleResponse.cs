using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Master.PointRules.Queries;

public class GetPointRuleResponse : Response
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal SpendAmount { get; set; }
    public int PointEarned { get; set; }
    public decimal? BonusMultiplier { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
