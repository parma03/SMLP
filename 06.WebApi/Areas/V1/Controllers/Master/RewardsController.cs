using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Rewards.Commands.Create;
using Smlp.Application.Rewards.Commands.Delete;
using Smlp.Application.Rewards.Commands.Update;
using Smlp.Application.Rewards.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Rewards.Commands.Create;
using Smlp.Shared.Master.Rewards.Commands.Update;
using Smlp.Shared.Master.Rewards.Constants;
using Smlp.Shared.Master.Rewards.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize]
public class RewardsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetRewardResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetRewardResponse>>> GetRewards([FromQuery] GetRewardRequest request)
    {
        return await Mediator.Send(new GetRewardByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.Rewards.RouteTemplateFor.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetRewardResponse))]
    public async Task<ActionResult<GetRewardResponse>> GetRewardById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetRewardByIdQuery { Id = id });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOwner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreateReward([FromBody] CreateRewardRequest request)
    {
        var result = await Mediator.Send(new CreateRewardCommand { Request = request });
        return CreatedAtAction(nameof(GetRewardById), new { id = result.Id }, result);
    }

    [HttpPut(ApiEndpoint.V1.Rewards.RouteTemplateFor.Update)]
    [Authorize(Policy = "AdminOrOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(CommonUpdateResponse))]
    public async Task<ActionResult<CommonUpdateResponse>> UpdateReward([FromRoute] Guid id, [FromBody] UpdateRewardRequest request)
    {
        return await Mediator.Send(new UpdateRewardCommand { Id = id, Request = request });
    }

    [HttpDelete(ApiEndpoint.V1.Rewards.RouteTemplateFor.Delete)]
    [Authorize(Policy = "AdminOrOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteReward([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteRewardCommand { Id = id });
        return Ok();
    }
}
