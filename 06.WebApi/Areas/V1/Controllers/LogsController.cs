using Microsoft.AspNetCore.Mvc;
using Psmp.Application.Logs.Commands;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;

namespace Psmp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
public class LogsController : ApiControllerBase
{
    [HttpPost]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CommonCreateResponse>> CreateHrisLog([FromForm] CreateHrisLogCommand command)
    {
        return CreatedAtAction(nameof(CreateHrisLog), await Mediator.Send(command));
    }
}
