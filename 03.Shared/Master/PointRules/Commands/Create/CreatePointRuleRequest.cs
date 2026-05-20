namespace Smlp.Shared.Master.PointRules.Commands.Create;

public class CreatePointRuleRequest
{
    public string Name { get; set; } = default!;
    public decimal SpendAmount { get; set; }
    public int PointEarned { get; set; }
    public decimal? BonusMultiplier { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
