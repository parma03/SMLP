using Microsoft.AspNetCore.Mvc;
using Smlp.Shared.Audits.Constants;
using Smlp.Shared.Audits.Queries.ExportAudits;
using Smlp.Shared.Audits.Queries.GetAudit;
using Smlp.Shared.Audits.Queries.GetAudits;
using Smlp.Shared.Common.Responses;

namespace Smlp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
public class AuditsController : ApiControllerBase
{
    [HttpGet]
    [Produces(typeof(PaginatedListResponse<GetAuditsAudit>))]
    public ActionResult<PaginatedListResponse<GetAuditsAudit>> GetAudits([FromQuery] GetAuditsRequest request)
    {
        return Ok(new PaginatedListResponse<GetAuditsAudit>());
    }

    [HttpGet(ApiEndpoint.V1.Audits.RouteTemplateFor.AuditId)]
    [Produces(typeof(GetAuditResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GetAuditResponse> GetAudit([FromRoute] Guid auditId)
    {
        return Ok(new GetAuditResponse { Id = auditId });
    }

    [HttpGet(ApiEndpoint.V1.Audits.RouteTemplateFor.Export)]
    [Produces(typeof(ExportAuditsResponse))]
    public ActionResult<ExportAuditsResponse> ExportAudits([FromQuery] IList<Guid> auditIds)
    {
        return Ok(new ExportAuditsResponse());
    }
}
