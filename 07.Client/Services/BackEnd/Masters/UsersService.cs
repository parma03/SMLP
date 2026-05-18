/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.Extensions.Options;
using Psmp.Client.Common.Extensions;
using Psmp.Client.Services.UserInfo;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Masters.AdministrativeCriterias.Commands;
using Psmp.Shared.Masters.AdministrativeCriterias.Queries;
using RestSharp;
using static Psmp.Shared.Masters.AdministrativeCriterias.Constants.ApiEndpoint.V1;

namespace Psmp.Client.Services.BackEnd.Masters;

public class UsersService : IDisposable
{
    private readonly RestClient _restClient = default!;

    public UsersService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo)
    {
        if (backEndServiceOptions is null)
        {
            return;
        }

        var options = new RestClientOptions(new Uri($"{backEndServiceOptions!.Value.BaseUrl}/{AdministrativeCriterias.Segment}"))
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };
        _restClient = new RestClient(options);
        _restClient.AddUserInfo(userInfo);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _restClient.Dispose();
    }

    public async Task<ResponseResult<ListResponse<GetAdministrativeCriteriaQueryResponse>>> GetAdministrativeCriteriaAsync(GetAdministrativeCriteriaQueryRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<ListResponse<GetAdministrativeCriteriaQueryResponse>>();
    }
    public async Task<ResponseResult<GetAdministrativeCriteriaQueryResponse>> GetAdministrativeCriteriaByIdAsync(int? id)
    {
        var restRequest = new RestRequest($"{nameof(AdministrativeCriterias.RouteTemplateFor.AdministrativeCriteriaById)}/{id}", Method.Get);
        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<GetAdministrativeCriteriaQueryResponse>();
    }
    public async Task<ResponseResult<AdministrativeCriteriaResponse>> CreateAdministrativeCriteriaAsync(AdministrativeCriteriaRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<AdministrativeCriteriaResponse>();
    }
    public async Task<ResponseResult<SuccessResponse>> UpdateAdministrativeCriteriaAsync(AdministrativeCriteriaRequest request)
    {
        var id = request.Id;

        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<SuccessResponse>();
    }
    public async Task<ResponseResult<SuccessResponse>> DeleteAdministrativeCriteriaAsync(int id)
    {
        var restRequest = new RestRequest($"{AdministrativeCriterias.RouteTemplateFor.AdministrativeCriteriaById}/{id}", Method.Delete);
        restRequest.AddParameters(id);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<SuccessResponse>();
    }
}
