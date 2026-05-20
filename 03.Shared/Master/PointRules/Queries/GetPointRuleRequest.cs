namespace Smlp.Shared.Master.PointRules.Queries;

public class GetPointRuleRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
