using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.PointAdjustments.Commands.Create;
using Smlp.Shared.Transaction.PointAdjustments.Queries;
using static Smlp.Shared.Transaction.PointAdjustments.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Transactions;

public class PointAdjustmentService : IDisposable
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public PointAdjustmentService(IOptions<BackEndOptions> options, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{PointAdjustments.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<PaginatedListResponse<GetPointAdjustmentResponse>>> GetAdjustmentsAsync(GetPointAdjustmentRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<PaginatedListResponse<GetPointAdjustmentResponse>>(_navigationManager);
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateAdjustmentAsync(CreatePointAdjustmentRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonCreateResponse>(_navigationManager);
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
