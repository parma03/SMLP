using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Auth.Commands.Login;
using Smlp.Application.Auth.Commands.Logout;
using Smlp.Application.Auth.Commands.RefreshToken;
using Smlp.Shared.Auth.Constants;
using Smlp.Shared.Auth.Responses;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;

namespace Smlp.WebApi.Areas.V1.Controllers.Auth;

public class AuthController : ApiControllerBase
{
    [HttpPost(ApiEndpoint.V1.Auths.RouteTemplateFor.Login)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces(typeof(LoginResponse))]
    public async Task<ActionResult<LoginResponse>> Login([FromForm] LoginCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost(ApiEndpoint.V1.Auths.RouteTemplateFor.RefreshToken)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces(typeof(RefreshTokenResponse))]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromForm] RefreshTokenCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost(ApiEndpoint.V1.Auths.RouteTemplateFor.Logout)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(SuccessResponse))]
    public async Task<ActionResult<SuccessResponse>> Logout()
    {
        return await Mediator.Send(new LogoutCommand());
    }
}
