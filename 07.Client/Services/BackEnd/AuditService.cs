using Microsoft.Extensions.Options;
using Psmp.Client.Common.Extensions;
using Psmp.Client.Services.UserInfo;
using Psmp.Shared.Audits.Queries.ExportAudits;
using Psmp.Shared.Audits.Queries.GetAudit;
using Psmp.Shared.Audits.Queries.GetAudits;
using Psmp.Shared.Common.Responses;
using RestSharp;
using static Psmp.Shared.Audits.Constants.ApiEndpoint.V1;

namespace Psmp.Client.Services.BackEnd;

public class AuditService
{
    private readonly RestClient _restClient;

    public AuditService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{backEndServiceOptions.Value.BaseUrl}/{Audits.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetAuditsAudit>>> GetAuditsAsync(GetAuditsRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<PaginatedListResponse<GetAuditsAudit>>();
    }

    public async Task<ResponseResult<GetAuditResponse>> GetAuditAsync(Guid auditId)
    {
        var restRequest = new RestRequest(auditId.ToString(), Method.Get);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<GetAuditResponse>();
    }

    public async Task<ResponseResult<ExportAuditsResponse>> ExportAuditsAsync(ExportAuditsRequest request)
    {
        var restRequest = new RestRequest(nameof(Audits.RouteTemplateFor.Export), Method.Get);
        restRequest.AddQueryParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<ExportAuditsResponse>();
    }
}
