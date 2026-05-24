using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.RedeemHistories.Commands.Create;
using Smlp.Shared.Transaction.RedeemHistories.Commands.Verify;
using Smlp.Shared.Transaction.RedeemHistories.Queries;
using static Smlp.Shared.Transaction.RedeemHistories.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Transactions;

public class RedeemService : IDisposable
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public RedeemService(IOptions<BackEndOptions> options, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{RedeemHistories.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<PaginatedListResponse<GetRedeemHistoryResponse>>> GetRedeemHistoriesAsync(GetRedeemHistoryRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<PaginatedListResponse<GetRedeemHistoryResponse>>(_navigationManager);
    }

    public async Task<ResponseResult<GetRedeemHistoryResponse>> GetRedeemByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<GetRedeemHistoryResponse>(_navigationManager);
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateRedeemAsync(CreateRedeemRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonCreateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<SuccessResponse>> VerifyRedeemAsync(Guid id, VerifyRedeemRequest request)
    {
        var restRequest = new RestRequest(RedeemHistories.RouteTemplateFor.Verify.Replace("{id:guid}", id.ToString()), Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<SuccessResponse>(_navigationManager);
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
