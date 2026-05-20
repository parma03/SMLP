using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.PointRules.Commands.Create;
using Smlp.Application.PointRules.Commands.Delete;
using Smlp.Application.PointRules.Commands.Update;
using Smlp.Application.PointRules.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.PointRules.Commands.Create;
using Smlp.Shared.Master.PointRules.Commands.Update;
using Smlp.Shared.Master.PointRules.Constants;
using Smlp.Shared.Master.PointRules.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize(Roles = "Admin,Owner")]
public class PointRulesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetPointRuleResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetPointRuleResponse>>> GetPointRules([FromQuery] GetPointRuleRequest request)
    {
        return await Mediator.Send(new GetPointRuleByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.PointRules.RouteTemplateFor.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetPointRuleResponse))]
    public async Task<ActionResult<GetPointRuleResponse>> GetPointRuleById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetPointRuleByIdQuery { Id = id });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreatePointRule([FromBody] CreatePointRuleRequest request)
    {
        var result = await Mediator.Send(new CreatePointRuleCommand { Request = request });
        return CreatedAtAction(nameof(GetPointRuleById), new { id = result.Id }, result);
    }

    [HttpPut(ApiEndpoint.V1.PointRules.RouteTemplateFor.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommonUpdateResponse>> UpdatePointRule([FromRoute] Guid id, [FromBody] UpdatePointRuleRequest request)
    {
        return await Mediator.Send(new UpdatePointRuleCommand { Id = id, Request = request });
    }

    [HttpDelete(ApiEndpoint.V1.PointRules.RouteTemplateFor.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePointRule([FromRoute] Guid id)
    {
        await Mediator.Send(new DeletePointRuleCommand { Id = id });
        return Ok();
    }
}
