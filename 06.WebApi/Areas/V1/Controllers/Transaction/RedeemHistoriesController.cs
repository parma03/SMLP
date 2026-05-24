using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.RedeemHistories.Commands.Create;
using Smlp.Application.RedeemHistories.Commands.Verify;
using Smlp.Application.RedeemHistories.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.RedeemHistories.Commands.Create;
using Smlp.Shared.Transaction.RedeemHistories.Commands.Verify;
using Smlp.Shared.Transaction.RedeemHistories.Constants;
using Smlp.Shared.Transaction.RedeemHistories.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Transactions;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize]
public class RedeemHistoriesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetRedeemHistoryResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetRedeemHistoryResponse>>> GetRedeemHistories([FromQuery] GetRedeemHistoryRequest request)
    {
        return await Mediator.Send(new GetRedeemHistoryByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.RedeemHistories.RouteTemplateFor.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetRedeemHistoryResponse))]
    public async Task<ActionResult<GetRedeemHistoryResponse>> GetRedeemHistoryById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetRedeemHistoryByIdQuery { Id = id });
    }

    [HttpPost]
    [Authorize(Policy = "AllRoles")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreateRedeem([FromBody] CreateRedeemRequest request)
    {
        var result = await Mediator.Send(new CreateRedeemCommand { Request = request });
        return CreatedAtAction(nameof(GetRedeemHistoryById), new { id = result.Id }, result);
    }

    [HttpPut(ApiEndpoint.V1.RedeemHistories.RouteTemplateFor.Verify)]
    [Authorize(Policy = "AdminOrKasir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(CommonUpdateResponse))]
    public async Task<ActionResult<CommonUpdateResponse>> VerifyRedeem([FromRoute] Guid id, [FromBody] VerifyRedeemRequest request)
    {
        return await Mediator.Send(new VerifyRedeemCommand { Id = id, Request = request });
    }
}
