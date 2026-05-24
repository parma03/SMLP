using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Audits.Queries;
using Smlp.Shared.Audits.Constants;
using Smlp.Shared.Audits.Queries.ExportAudits;
using Smlp.Shared.Audits.Queries.GetAudit;
using Smlp.Shared.Audits.Queries.GetAudits;
using Smlp.Shared.Common.Responses;

namespace Smlp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize(Roles = "Admin,Owner")]
public class AuditsController : ApiControllerBase
{
    [HttpGet]
    [Produces(typeof(PaginatedListResponse<GetAuditsAudit>))]
    public async Task<ActionResult<PaginatedListResponse<GetAuditsAudit>>> GetAudits([FromQuery] GetAuditsRequest request)
    {
        return await Mediator.Send(new GetAuditsQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.Audits.RouteTemplateFor.AuditId)]
    [Produces(typeof(GetAuditResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetAuditResponse>> GetAudit([FromRoute] Guid auditId)
    {
        return await Mediator.Send(new GetAuditByIdQuery { Id = auditId });
    }

    [HttpGet(ApiEndpoint.V1.Audits.RouteTemplateFor.Export)]
    [Produces(typeof(ExportAuditsResponse))]
    public async Task<ActionResult<ExportAuditsResponse>> ExportAudits([FromQuery] IList<Guid> auditIds)
    {
        return await Mediator.Send(new ExportAuditsQuery { AuditIds = auditIds });
    }
}
