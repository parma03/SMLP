using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Products.Commands.Create;
using Smlp.Application.Products.Commands.Delete;
using Smlp.Application.Products.Commands.Update;
using Smlp.Application.Products.Queries;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
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

    /// <summary>Serve file gambar produk dari storage.</summary>
    [HttpGet("{id:guid}/image")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductImage(
        [FromRoute] Guid id,
        [FromServices] ISmlpDbContext context,
        [FromServices] IStorageService storage,
        CancellationToken cancellationToken)
    {
        var product = await context.Products
            .AsNoTracking()
            .Select(p => new { p.Id, p.ProductImage, p.IsDeleted })
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (product?.ProductImage is null)
            return NotFound();

        try
        {
            var bytes = await storage.ReadAsync(product.ProductImage.StorageFileId);
            var contentType = string.IsNullOrWhiteSpace(product.ProductImage.FileContentType)
                ? "image/jpeg"
                : product.ProductImage.FileContentType;
            return File(bytes, contentType, enableRangeProcessing: true);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOwner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var result = await Mediator.Send(new CreateProductCommand { Request = request });
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    [HttpPut(ApiEndpoint.V1.Products.RouteTemplateFor.Update)]
    [Authorize(Policy = "AdminOrOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(CommonUpdateResponse))]
    public async Task<ActionResult<CommonUpdateResponse>> UpdateProduct([FromRoute] Guid id, [FromBody] UpdateProductRequest request)
    {
        return await Mediator.Send(new UpdateProductCommand { Id = id, Request = request });
    }

    [HttpDelete(ApiEndpoint.V1.Products.RouteTemplateFor.Delete)]
    [Authorize(Policy = "AdminOrOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProduct([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteProductCommand { Id = id });
        return Ok();
    }
}
