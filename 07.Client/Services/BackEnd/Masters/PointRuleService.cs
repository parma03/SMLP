using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
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

    public PointRuleService(IOptions<BackEndOptions> options, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{PointRules.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetPointRuleResponse>>> GetPointRulesAsync(GetPointRuleRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<PaginatedListResponse<GetPointRuleResponse>>();
    }

    public async Task<ResponseResult<GetPointRuleResponse>> GetPointRuleByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<GetPointRuleResponse>();
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreatePointRuleAsync(CreatePointRuleRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonCreateResponse>();
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdatePointRuleAsync(Guid id, UpdatePointRuleRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonUpdateResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> DeletePointRuleAsync(Guid id)
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
