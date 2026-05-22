using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
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

    public TierRuleService(IOptions<BackEndOptions> options, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{TierRules.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetTierRuleResponse>>> GetTierRulesAsync(GetTierRuleRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<PaginatedListResponse<GetTierRuleResponse>>();
    }

    public async Task<ResponseResult<GetTierRuleResponse>> GetTierRuleByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<GetTierRuleResponse>();
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateTierRuleAsync(CreateTierRuleRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonCreateResponse>();
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdateTierRuleAsync(Guid id, UpdateTierRuleRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonUpdateResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteTierRuleAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Delete);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<SuccessResponse>();
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
