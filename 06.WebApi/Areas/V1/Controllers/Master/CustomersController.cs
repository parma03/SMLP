using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Customers.Commands.Create;
using Smlp.Application.Customers.Commands.Delete;
using Smlp.Application.Customers.Commands.Update;
using Smlp.Application.Customers.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Customers.Commands.Create;
using Smlp.Shared.Master.Customers.Commands.Update;
using Smlp.Shared.Master.Customers.Constants;
using Smlp.Shared.Master.Customers.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize]
public class CustomersController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetCustomerResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetCustomerResponse>>> GetCustomers([FromQuery] GetCustomerRequest request)
    {
        return await Mediator.Send(new GetCustomerByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.Customers.RouteTemplateFor.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetCustomerResponse))]
    public async Task<ActionResult<GetCustomerResponse>> GetCustomerById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetCustomerByIdQuery { Id = id });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrKasir")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var result = await Mediator.Send(new CreateCustomerCommand { Request = request });
        return CreatedAtAction(nameof(GetCustomerById), new { id = result.Id }, result);
    }

    [HttpPut(ApiEndpoint.V1.Customers.RouteTemplateFor.Update)]
    [Authorize(Policy = "AdminOrKasir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(CommonUpdateResponse))]
    public async Task<ActionResult<CommonUpdateResponse>> UpdateCustomer([FromRoute] Guid id, [FromBody] UpdateCustomerRequest request)
    {
        return await Mediator.Send(new UpdateCustomerCommand { Id = id, Request = request });
    }

    [HttpDelete(ApiEndpoint.V1.Customers.RouteTemplateFor.Delete)]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCustomer([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteCustomerCommand { Id = id });
        return Ok();
    }
}
