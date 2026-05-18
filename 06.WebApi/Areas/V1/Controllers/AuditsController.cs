using Microsoft.AspNetCore.Mvc;
using Psmp.Application.Audits.Queries.ExportAudits;
using Psmp.Application.Audits.Queries.GetAudit;
using Psmp.Application.Audits.Queries.GetAudits;
using Psmp.Shared.Audits.Constants;
using Psmp.Shared.Audits.Queries.ExportAudits;
using Psmp.Shared.Audits.Queries.GetAudit;
using Psmp.Shared.Audits.Queries.GetAudits;
using Psmp.Shared.Common.Responses;
using Psmp.WebApi.Common.Extensions;

namespace Psmp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
public class AuditsController : ApiControllerBase
{
    [HttpGet]
    [Produces(typeof(PaginatedListResponse<GetAuditsAudit>))]
    public async Task<ActionResult<PaginatedListResponse<GetAuditsAudit>>> GetAudits([FromQuery] GetAuditsQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpGet(ApiEndpoint.V1.Audits.RouteTemplateFor.AuditId)]
    [Produces(typeof(GetAuditResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetAuditResponse>> GetAudit([FromRoute] Guid auditId)
    {
        return await Mediator.Send(new GetAuditQuery { AuditId = auditId });
    }

    [HttpGet(ApiEndpoint.V1.Audits.RouteTemplateFor.Export)]
    [Produces(typeof(ExportAuditsResponse))]
    public async Task<ActionResult> ExportAudits([FromQuery] IList<Guid> auditIds)
    {
        var query = new ExportAuditsQuery
        {
            AuditIds = auditIds
        };

        var response = await Mediator.Send(query);

        return response.AsFile();
    }
}
