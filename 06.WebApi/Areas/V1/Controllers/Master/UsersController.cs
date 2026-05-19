/*
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Applicants.Commands.CancelApplicant;
using Smlp.Application.Applicants.Commands.CreateApplicant;
using Smlp.Application.Applicants.Commands.CreateApplicantAdministrativeCriteria;
using Smlp.Application.Applicants.Queries.GetApplicant;
using Smlp.Application.Applicants.Queries.GetApplicantPayment;
using Smlp.Application.Applicants.Queries.GetMentor;
using Smlp.Shared.Applicants.Commands.Create;
using Smlp.Shared.Applicants.Constants;
using Smlp.Shared.Applicants.Queries.GetApplicant;
using Smlp.Shared.Applicants.Queries.GetApplicantPayment;
using Smlp.Shared.Applicants.Queries.GetMentor;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;

namespace Smlp.WebApi.Areas.V1.Controllers.Applicants;

[ApiVersion(ApiVersioning.V1.Number)]
public class UsersController : ApiControllerBase
{
    [HttpPost]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateApplicantRequest>> CreateApplicant([FromForm] CreateApplicantCommand command)
    {
        return CreatedAtAction(nameof(CreateApplicant), await Mediator.Send(command));
    }

    [HttpPost(ApiEndpoint.V1.Applicants.RouteTemplateFor.CreateApplicantAdministrativeCriteria)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CommonCreateResponse>> CreateApplicantAdministrativeCriteria([FromForm] CreateApplicantAdministrativeCriteriaCommand command)
    {
        return await Mediator.Send(command);
    }

    [AllowAnonymous]
    [HttpGet(ApiEndpoint.V1.Applicants.RouteTemplateFor.GetApplicantByScholarshipEmployeeId)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(GetApplicantByScholarshipEmployeeIdResponse))]
    public async Task<ActionResult<GetApplicantByScholarshipEmployeeIdResponse>> GetApplicantByScholarshipEmployeeId([FromRoute] Guid scholarshipId, string employeeId)
    {
        return await Mediator.Send(new GetApplicantByScholarshipEmployeeIdQuery { ScholarshipId = scholarshipId, EmployeeId = employeeId });
    }

    [AllowAnonymous]
    [HttpGet(ApiEndpoint.V1.Applicants.RouteTemplateFor.GetApplicantByEmployeeId)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(ListResponse<GetApplicantByEmployeeIdResponse>))]
    public async Task<ActionResult<ListResponse<GetApplicantByEmployeeIdResponse>>> GetApplicantByEmployeeId([FromRoute] string employeeId)
    {
        return await Mediator.Send(new GetApplicantByEmployeeIdQuery { EmployeeId = employeeId });
    }
    [AllowAnonymous]
    [HttpGet(ApiEndpoint.V1.Applicants.RouteTemplateFor.GetApplicantById)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(ListResponse<GetApplicantByEmployeeIdResponse>))]
    public async Task<ActionResult<ListResponse<GetApplicantByEmployeeIdResponse>>> GetApplicantById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetApplicantByIdQuery { Id = id });
    }

    [AllowAnonymous]
    [HttpGet(ApiEndpoint.V1.Applicants.RouteTemplateFor.GetMentorByEmployeeId)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(GetMentorResponse))]
    public async Task<ActionResult<GetMentorResponse>> GetMentorByEmployeeId([FromRoute] string employeeId)
    {
        return await Mediator.Send(new GetMentorQuery { EmployeeId = employeeId });
    }

    [HttpGet(ApiEndpoint.V1.Applicants.RouteTemplateFor.GetApplicantPaymentDibursement)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(ListResponse<GetApplicantPaymentDibursementResponse>))]
    public async Task<ActionResult<ListResponse<GetApplicantPaymentDibursementResponse>>> GetApplicantPaymentDibursement([FromQuery] GetApplicantPaymentDibursementQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost(ApiEndpoint.V1.Applicants.RouteTemplateFor.CancelApplicant)]
    [Consumes(ContentTypes.ApplicationXWwwFormUrlEncoded)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommonCreateResponse>> CancelApplicant([FromForm] CancelApplicantCommand command)
    {
        return await Mediator.Send(command);
    }
}
