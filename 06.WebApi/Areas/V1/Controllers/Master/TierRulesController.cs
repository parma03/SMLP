using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.TierRules.Commands.Create;
using Smlp.Application.TierRules.Commands.Delete;
using Smlp.Application.TierRules.Commands.Update;
using Smlp.Application.TierRules.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.TierRules.Commands.Create;
using Smlp.Shared.Master.TierRules.Commands.Update;
using Smlp.Shared.Master.TierRules.Constants;
using Smlp.Shared.Master.TierRules.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize(Policy = "AdminOrOwner")]
public class TierRulesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetTierRuleResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetTierRuleResponse>>> GetTierRules([FromQuery] GetTierRuleRequest request)
    {
        return await Mediator.Send(new GetTierRuleByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.TierRules.RouteTemplateFor.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetTierRuleResponse))]
    public async Task<ActionResult<GetTierRuleResponse>> GetTierRuleById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetTierRuleByIdQuery { Id = id });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreateTierRule([FromBody] CreateTierRuleRequest request)
    {
        var result = await Mediator.Send(new CreateTierRuleCommand { Request = request });
        return CreatedAtAction(nameof(GetTierRuleById), new { id = result.Id }, result);
    }

    [HttpPut(ApiEndpoint.V1.TierRules.RouteTemplateFor.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommonUpdateResponse>> UpdateTierRule([FromRoute] Guid id, [FromBody] UpdateTierRuleRequest request)
    {
        return await Mediator.Send(new UpdateTierRuleCommand { Id = id, Request = request });
    }

    [HttpDelete(ApiEndpoint.V1.TierRules.RouteTemplateFor.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTierRule([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteTierRuleCommand { Id = id });
        return Ok();
    }
}
