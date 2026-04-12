namespace Smlp.Shared.Audits.Queries.GetAudits;

public class GetAuditsAudit
{
    public Guid Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string ActionType { get; set; } = default!;
    public string FromIpAddress { get; set; } = default!;
}
