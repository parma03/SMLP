using Microsoft.AspNetCore.Mvc;
using Psmp.Application.Emails.Commands;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Emails.Commands.SendEmail;
using Psmp.Shared.Emails.Constants;

namespace Psmp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
public class EmailsController : ApiControllerBase
{
    [HttpPost]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    public async Task<ActionResult<ListResponse<SendEmailResponse>>> SendEmail([FromForm] SendEmailCommand command)
    {
        return CreatedAtAction(nameof(SendEmail), await Mediator.Send(command));
    }

    [HttpPost(ApiEndpoint.V1.Emails.RouteTemplateFor.SendEmailScheduleToefl)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    public async Task<ActionResult<ListResponse<SendEmailResponse>>> SendEmailScheduleToefl([FromForm] SendEmailScheduleToeflCommand command)
    {
        return CreatedAtAction(nameof(SendEmailScheduleToefl), await Mediator.Send(command));
    }

    [HttpPost(ApiEndpoint.V1.Emails.RouteTemplateFor.SendEmailSchedulePsychoTest)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    public async Task<ActionResult<ListResponse<SendEmailResponse>>> SendEmailSchedulePsychoTest([FromForm] SendEmailSchedulePsychoTestCommand command)
    {
        return CreatedAtAction(nameof(SendEmailSchedulePsychoTest), await Mediator.Send(command));
    }

    [HttpPost(ApiEndpoint.V1.Emails.RouteTemplateFor.SendEmailSponsorshipLetter)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    public async Task<ActionResult<ListResponse<SendEmailResponse>>> SendEmailSponsorshipLetter([FromForm] SendEmailSponsorshipLetterCommand command)
    {
        return CreatedAtAction(nameof(SendEmailSponsorshipLetter), await Mediator.Send(command));
    }

    [HttpPost(ApiEndpoint.V1.Emails.RouteTemplateFor.SendEmailVisaGuarantee)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    public async Task<ActionResult<ListResponse<SendEmailResponse>>> SendEmailVisaGuarantee([FromForm] SendEmailVisaGuaranteeCommand command)
    {
        return CreatedAtAction(nameof(SendEmailVisaGuarantee), await Mediator.Send(command));
    }
}
