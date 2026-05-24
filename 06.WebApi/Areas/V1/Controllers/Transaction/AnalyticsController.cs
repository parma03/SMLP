using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Analytics.Queries;
using Smlp.Shared.Analytics.Constants;
using Smlp.Shared.Analytics.Queries.GetCustomerInsights;
using Smlp.Shared.Analytics.Queries.GetDashboardSummary;
using Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;
using Smlp.Shared.Analytics.Queries.GetRevenueChart;

namespace Smlp.WebApi.Areas.V1.Controllers.Transaction;

[ApiVersion(ApiVersioning.V1.Number)]
[Authorize(Policy = "AdminOrOwner")]
public class AnalyticsController : ApiControllerBase
{
    /// <summary>Ringkasan dashboard utama: KPI, tier, poin beredar</summary>
    [HttpGet(ApiEndpoint.V1.Analytics.RouteTemplateFor.Dashboard)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(GetDashboardSummaryResponse))]
    public async Task<ActionResult<GetDashboardSummaryResponse>> GetDashboardSummary()
    {
        return await Mediator.Send(new GetDashboardSummaryQuery());
    }

    /// <summary>Grafik revenue bulanan/harian dan breakdown metode bayar</summary>
    [HttpGet(ApiEndpoint.V1.Analytics.RouteTemplateFor.Revenue)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(GetRevenueChartResponse))]
    public async Task<ActionResult<GetRevenueChartResponse>> GetRevenueChart([FromQuery] GetRevenueChartRequest request)
    {
        return await Mediator.Send(new GetRevenueChartQuery { Request = request });
    }

    /// <summary>Analisis pelanggan: RFM, churn risk, top spender, ulang tahun</summary>
    [HttpGet(ApiEndpoint.V1.Analytics.RouteTemplateFor.CustomerInsights)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(GetCustomerInsightsResponse))]
    public async Task<ActionResult<GetCustomerInsightsResponse>> GetCustomerInsights()
    {
        return await Mediator.Send(new GetCustomerInsightsQuery());
    }

    /// <summary>Analisis loyalitas: ekonomi poin, performa reward, pergerakan tier</summary>
    [HttpGet(ApiEndpoint.V1.Analytics.RouteTemplateFor.LoyaltyAnalysis)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(GetLoyaltyAnalysisResponse))]
    public async Task<ActionResult<GetLoyaltyAnalysisResponse>> GetLoyaltyAnalysis()
    {
        return await Mediator.Send(new GetLoyaltyAnalysisQuery());
    }
}
