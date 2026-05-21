/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Users.Commands.Create;
using Smlp.Application.Users.Commands.Delete;
using Smlp.Application.Users.Commands.Update;
using Smlp.Application.Users.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Users.Commands.Create;
using Smlp.Shared.Master.Users.Commands.Update;
using Smlp.Shared.Master.Users.Constants;
using Smlp.Shared.Master.Users.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetUserResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetUserResponse>>> GetUsers([FromQuery] GetUserRequest request)
    {
        return await Mediator.Send(new GetUserByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.Users.RouteTemplateFor.GetUserById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetUserResponse))]
    public async Task<ActionResult<GetUserResponse>> GetUserById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetUserByIdQuery { Id = id });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        return CreatedAtAction(nameof(GetUserById), await Mediator.Send(new CreateUserCommand { Request = request }));
    }

    /// <summary>
    /// Update data user. Untuk mengubah foto profil, gunakan multipart/form-data
    /// dan sertakan file di field ProfilePictureFile.
    /// </summary>
    [HttpPut(ApiEndpoint.V1.Users.RouteTemplateFor.UpdateUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(CommonUpdateResponse))]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CommonUpdateResponse>> UpdateUser([FromRoute] Guid id, [FromForm] UpdateUserRequest request)
    {
        return await Mediator.Send(new UpdateUserCommand { Id = id, Request = request });
    }

    [HttpDelete(ApiEndpoint.V1.Users.RouteTemplateFor.DeleteUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteUserCommand { Id = id });
        return Ok();
    }
}
