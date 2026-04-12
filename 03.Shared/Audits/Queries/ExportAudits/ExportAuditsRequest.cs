using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Audits.Queries.ExportAudits;

public class ExportAuditsRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public IList<Guid> AuditIds { get; set; } = new List<Guid>();
}
