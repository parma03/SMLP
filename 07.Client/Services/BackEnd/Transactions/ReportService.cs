using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Reports.Queries.GetPointReport;
using Smlp.Shared.Reports.Queries.GetRedeemReport;
using Smlp.Shared.Reports.Queries.GetTransactionReport;
using static Smlp.Shared.Reports.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Transactions;

public class ReportService : IDisposable
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public ReportService(IOptions<BackEndOptions> options, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{Reports.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<PaginatedListResponse<GetTransactionReportResponse>>> GetTransactionReportAsync(GetTransactionReportRequest request)
    {
        var restRequest = new RestRequest(Reports.RouteTemplateFor.Transactions, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<PaginatedListResponse<GetTransactionReportResponse>>(_navigationManager);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetRedeemReportResponse>>> GetRedeemReportAsync(GetRedeemReportRequest request)
    {
        var restRequest = new RestRequest(Reports.RouteTemplateFor.Redeems, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<PaginatedListResponse<GetRedeemReportResponse>>(_navigationManager);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetPointReportResponse>>> GetPointReportAsync(GetPointReportRequest request)
    {
        var restRequest = new RestRequest(Reports.RouteTemplateFor.Points, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<PaginatedListResponse<GetPointReportResponse>>(_navigationManager);
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
