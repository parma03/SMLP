using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.PointAdjustments.Commands.Create;
using Smlp.Application.PointAdjustments.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.PointAdjustments.Commands.Create;
using Smlp.Shared.Transaction.PointAdjustments.Constants;
using Smlp.Shared.Transaction.PointAdjustments.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Transactions;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize(Policy = "AdminOrOwner")]
public class PointAdjustmentsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetPointAdjustmentResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetPointAdjustmentResponse>>> GetPointAdjustments([FromQuery] GetPointAdjustmentRequest request)
    {
        return await Mediator.Send(new GetPointAdjustmentByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.PointAdjustments.RouteTemplateFor.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetPointAdjustmentResponse))]
    public async Task<ActionResult<GetPointAdjustmentResponse>> GetPointAdjustmentById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetPointAdjustmentByIdQuery { Id = id });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreatePointAdjustment([FromBody] CreatePointAdjustmentRequest request)
    {
        var result = await Mediator.Send(new CreatePointAdjustmentCommand { Request = request });
        return CreatedAtAction(nameof(GetPointAdjustmentById), new { id = result.Id }, result);
    }
}
