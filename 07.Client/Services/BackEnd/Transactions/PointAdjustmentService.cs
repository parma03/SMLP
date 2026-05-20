using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.PointAdjustments.Commands.Create;
using Smlp.Shared.Transaction.PointAdjustments.Queries;
using static Smlp.Shared.Transaction.PointAdjustments.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Transactions;

public class PointAdjustmentService : IDisposable
{
    private readonly RestClient _restClient;

    public PointAdjustmentService(IOptions<BackEndOptions> options, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{PointAdjustments.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetPointAdjustmentResponse>>> GetAdjustmentsAsync(GetPointAdjustmentRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<PaginatedListResponse<GetPointAdjustmentResponse>>();
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateAdjustmentAsync(CreatePointAdjustmentRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonCreateResponse>();
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
