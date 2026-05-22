using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Products.Commands.Create;
using Smlp.Application.Products.Commands.Delete;
using Smlp.Application.Products.Commands.Update;
using Smlp.Application.Products.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Products.Commands.Create;
using Smlp.Shared.Master.Products.Commands.Update;
using Smlp.Shared.Master.Products.Constants;
using Smlp.Shared.Master.Products.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize]
public class ProductsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetProductResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetProductResponse>>> GetProducts([FromQuery] GetProductRequest request)
    {
        return await Mediator.Send(new GetProductByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.Products.RouteTemplateFor.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetProductResponse))]
    public async Task<ActionResult<GetProductResponse>> GetProductById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetProductByIdQuery { Id = id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var result = await Mediator.Send(new CreateProductCommand { Request = request });
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    [HttpPut(ApiEndpoint.V1.Products.RouteTemplateFor.Update)]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(CommonUpdateResponse))]
    public async Task<ActionResult<CommonUpdateResponse>> UpdateProduct([FromRoute] Guid id, [FromBody] UpdateProductRequest request)
    {
        return await Mediator.Send(new UpdateProductCommand { Id = id, Request = request });
    }

    [HttpDelete(ApiEndpoint.V1.Products.RouteTemplateFor.Delete)]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProduct([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteProductCommand { Id = id });
        return Ok();
    }
}
