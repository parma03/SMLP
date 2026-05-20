using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Transactions.Commands.Create;
using Smlp.Application.Transactions.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.Transactions.Commands.Create;
using Smlp.Shared.Transaction.Transactions.Constants;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.WebApi.Areas.V1.Controllers.Transactions;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize]
public class TransactionsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetTransactionResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetTransactionResponse>>> GetTransactions([FromQuery] GetTransactionRequest request)
    {
        return await Mediator.Send(new GetTransactionByListQuery { Request = request });
    }

    [HttpGet(ApiEndpoint.V1.Transactions.RouteTemplateFor.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GetTransactionResponse))]
    public async Task<ActionResult<GetTransactionResponse>> GetTransactionById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetTransactionByIdQuery { Id = id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Kasir")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(CommonCreateResponse))]
    public async Task<ActionResult<CommonCreateResponse>> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        var result = await Mediator.Send(new CreateTransactionCommand { Request = request });
        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Id }, result);
    }
}
