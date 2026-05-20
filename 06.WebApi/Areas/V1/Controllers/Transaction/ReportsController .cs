using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Reports.Queries;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Reports.Constants;
using Smlp.Shared.Reports.Queries.GetPointReport;
using Smlp.Shared.Reports.Queries.GetRedeemReport;
using Smlp.Shared.Reports.Queries.GetTransactionReport;

namespace Smlp.WebApi.Areas.V1.Controllers.Transaction;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize(Roles = "Admin,Owner")]
public class ReportsController : ApiControllerBase
{
    /// <summary>Laporan transaksi dengan filter tanggal dan pelanggan</summary>
    [HttpGet(ApiEndpoint.V1.Reports.RouteTemplateFor.Transactions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetTransactionReportResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetTransactionReportResponse>>> GetTransactionReport([FromQuery] GetTransactionReportRequest request)
    {
        return await Mediator.Send(new GetTransactionReportQuery { Request = request });
    }

    /// <summary>Laporan penukaran reward</summary>
    [HttpGet(ApiEndpoint.V1.Reports.RouteTemplateFor.Redeems)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetRedeemReportResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetRedeemReportResponse>>> GetRedeemReport([FromQuery] GetRedeemReportRequest request)
    {
        return await Mediator.Send(new GetRedeemReportQuery { Request = request });
    }

    /// <summary>Laporan aktivitas poin (earn, redeem, adjustment)</summary>
    [HttpGet(ApiEndpoint.V1.Reports.RouteTemplateFor.Points)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PaginatedListResponse<GetPointReportResponse>))]
    public async Task<ActionResult<PaginatedListResponse<GetPointReportResponse>>> GetPointReport([FromQuery] GetPointReportRequest request)
    {
        return await Mediator.Send(new GetPointReportQuery { Request = request });
    }
}
