using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Analytics.Queries.ExportAnalytics;
using Smlp.Shared.Analytics.Queries.GetCustomerInsights;
using Smlp.Shared.Analytics.Queries.GetDashboardSummary;
using Smlp.Shared.Analytics.Queries.GetLoyaltyAnalysis;
using Smlp.Shared.Analytics.Queries.GetRevenueChart;
using Smlp.Shared.Common.Responses;
using static Smlp.Shared.Analytics.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Transactions;

public class AnalyticsService : IDisposable
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public AnalyticsService(IOptions<BackEndOptions> options, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{Analytics.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<GetDashboardSummaryResponse>> GetDashboardSummaryAsync()
    {
        var request = new RestRequest(Analytics.RouteTemplateFor.Dashboard, Method.Get);
        return (await _restClient.ExecuteAsync(request)).ToResponseResultWithRedirect<GetDashboardSummaryResponse>(_navigationManager);
    }

    public async Task<ResponseResult<GetRevenueChartResponse>> GetRevenueChartAsync(GetRevenueChartRequest req)
    {
        var request = new RestRequest(Analytics.RouteTemplateFor.Revenue, Method.Get);
        request.AddQueryParameter("year", req.Year.ToString());
        if (req.Month > 0)
            request.AddQueryParameter("month", req.Month.ToString());
        return (await _restClient.ExecuteAsync(request)).ToResponseResultWithRedirect<GetRevenueChartResponse>(_navigationManager);
    }

    public async Task<ResponseResult<GetCustomerInsightsResponse>> GetCustomerInsightsAsync()
    {
        var request = new RestRequest(Analytics.RouteTemplateFor.CustomerInsights, Method.Get);
        return (await _restClient.ExecuteAsync(request)).ToResponseResultWithRedirect<GetCustomerInsightsResponse>(_navigationManager);
    }

    public async Task<ResponseResult<GetLoyaltyAnalysisResponse>> GetLoyaltyAnalysisAsync()
    {
        var request = new RestRequest(Analytics.RouteTemplateFor.LoyaltyAnalysis, Method.Get);
        return (await _restClient.ExecuteAsync(request)).ToResponseResultWithRedirect<GetLoyaltyAnalysisResponse>(_navigationManager);
    }

    public async Task<ResponseResult<ExportAnalyticsResponse>> ExportAsync(ExportAnalyticsRequest req)
    {
        var request = new RestRequest(Analytics.RouteTemplateFor.Export, Method.Get);
        request.AddQueryParameter("type", req.Type);
        request.AddQueryParameter("year", req.Year.ToString());
        if (req.Month > 0)
            request.AddQueryParameter("month", req.Month.ToString());
        return (await _restClient.ExecuteAsync(request)).ToResponseResultWithRedirect<ExportAnalyticsResponse>(_navigationManager);
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
