using Smlp.Base.ValueObjects;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Audits.Queries.GetAudit;

public class GetAuditResponse : Response
{
    public Guid Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string ActionType { get; set; } = default!;
    public string ActionName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string? OldValues { get; set; }
    public string NewValues { get; set; } = default!;
    public string ClientApplicationId { get; set; } = default!;
    public string FromIpAddress { get; set; } = default!;
    public Geolocation? FromGeolocation { get; set; }
}
