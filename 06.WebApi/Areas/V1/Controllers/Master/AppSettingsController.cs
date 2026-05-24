using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.AppSettings.Commands.Upsert;
using Smlp.Application.AppSettings.Queries;
using Smlp.Shared.AppSettings.Commands.UpsertAppSettings;
using Smlp.Shared.AppSettings.Queries.GetAppSettings;
using static Smlp.Shared.AppSettings.Constants.ApiEndpoint;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize(Policy = "AdminOrOwner")]
public class AppSettingsController : ApiControllerBase
{
    /// <summary>
    /// Mengambil seluruh pengaturan aplikasi dari database.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(GetAppSettingsResponse))]
    public async Task<ActionResult<GetAppSettingsResponse>> GetAppSettings()
    {
        return await Mediator.Send(new GetAppSettingsQuery());
    }

    /// <summary>
    /// Menyimpan (upsert) pengaturan aplikasi ke database.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertAppSettings([FromBody] UpsertAppSettingsRequest request)
    {
        await Mediator.Send(new UpsertAppSettingsCommand { Request = request });
        return NoContent();
    }
}
