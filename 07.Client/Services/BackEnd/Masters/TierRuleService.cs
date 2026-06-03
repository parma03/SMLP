using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.TierRules.Commands.Create;
using Smlp.Shared.Master.TierRules.Commands.Update;
using Smlp.Shared.Master.TierRules.Queries;
using static Smlp.Shared.Master.TierRules.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Masters;

public class TierRuleService : IDisposable
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public TierRuleService(IOptions<BackEndOptions> options, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{TierRules.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<PaginatedListResponse<GetTierRuleResponse>>> GetTierRulesAsync(GetTierRuleRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        // GET: gunakan retry otomatis (idempotent)
        return await _restClient.ExecuteWithRetryAndRedirectAsync<PaginatedListResponse<GetTierRuleResponse>>(restRequest, _navigationManager);
    }

    public async Task<ResponseResult<GetTierRuleResponse>> GetTierRuleByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        // GET: gunakan retry otomatis (idempotent)
        return await _restClient.ExecuteWithRetryAndRedirectAsync<GetTierRuleResponse>(restRequest, _navigationManager);
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateTierRuleAsync(CreateTierRuleRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonCreateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdateTierRuleAsync(Guid id, UpdateTierRuleRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonUpdateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteTierRuleAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Delete);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<SuccessResponse>(_navigationManager);
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
