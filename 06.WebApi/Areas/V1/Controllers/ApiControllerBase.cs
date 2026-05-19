using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Smlp.WebApi.Areas.V1.Controllers;

[Route(ApiVersioning.V1.BaseRoute)]
[ApiVersion(ApiVersioning.V1.Number)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender _mediator = default!;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
