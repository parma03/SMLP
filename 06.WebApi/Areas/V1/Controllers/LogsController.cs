using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Logs.Commands;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;

namespace Smlp.WebApi.Areas.V1.Controllers;

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
