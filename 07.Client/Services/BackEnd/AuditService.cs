using Microsoft.Extensions.Options;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Audits.Queries.ExportAudits;
using Smlp.Shared.Audits.Queries.GetAudit;
using Smlp.Shared.Audits.Queries.GetAudits;
using Smlp.Shared.Common.Responses;
using RestSharp;
using static Smlp.Shared.Audits.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd;

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
