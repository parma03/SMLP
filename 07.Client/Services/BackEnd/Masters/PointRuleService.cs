using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.PointRules.Commands.Create;
using Smlp.Shared.Master.PointRules.Commands.Update;
using Smlp.Shared.Master.PointRules.Queries;
using static Smlp.Shared.Master.PointRules.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Masters;

public class PointRuleService : IDisposable
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public PointRuleService(IOptions<BackEndOptions> options, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{PointRules.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<PaginatedListResponse<GetPointRuleResponse>>> GetPointRulesAsync(GetPointRuleRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        // GET: gunakan retry otomatis (idempotent)
        return await _restClient.ExecuteWithRetryAndRedirectAsync<PaginatedListResponse<GetPointRuleResponse>>(restRequest, _navigationManager);
    }

    public async Task<ResponseResult<GetPointRuleResponse>> GetPointRuleByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        // GET: gunakan retry otomatis (idempotent)
        return await _restClient.ExecuteWithRetryAndRedirectAsync<GetPointRuleResponse>(restRequest, _navigationManager);
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreatePointRuleAsync(CreatePointRuleRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        // POST: tidak di-retry (non-idempotent)
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonCreateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdatePointRuleAsync(Guid id, UpdatePointRuleRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        // PUT: tidak di-retry (non-idempotent)
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonUpdateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<SuccessResponse>> DeletePointRuleAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Delete);
        // DELETE: tidak di-retry (non-idempotent)
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<SuccessResponse>(_navigationManager);
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}

