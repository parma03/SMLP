/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Emails.Commands;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Emails.Commands.SendEmail;

namespace Smlp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
public class EmailsController : ApiControllerBase
{
    /// <summary>Kirim email notifikasi (via Gmail SMTP).</summary>
    [HttpPost]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(ListResponse<SendEmailResponse>))]
    public async Task<ActionResult<ListResponse<SendEmailResponse>>> SendEmail([FromForm] SendEmailCommand command)
    {
        return CreatedAtAction(nameof(SendEmail), await Mediator.Send(command));
    }
}
