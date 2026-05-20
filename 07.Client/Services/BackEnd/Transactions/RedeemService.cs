using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
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

    public RedeemService(IOptions<BackEndOptions> options, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{RedeemHistories.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetRedeemHistoryResponse>>> GetRedeemHistoriesAsync(GetRedeemHistoryRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<PaginatedListResponse<GetRedeemHistoryResponse>>();
    }

    public async Task<ResponseResult<GetRedeemHistoryResponse>> GetRedeemByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<GetRedeemHistoryResponse>();
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateRedeemAsync(CreateRedeemRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonCreateResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> VerifyRedeemAsync(Guid id, VerifyRedeemRequest request)
    {
        var restRequest = new RestRequest(RedeemHistories.RouteTemplateFor.Verify.Replace("{id:guid}", id.ToString()), Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<SuccessResponse>();
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
