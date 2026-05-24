using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Customers.Commands.Create;
using Smlp.Shared.Master.Customers.Commands.Update;
using Smlp.Shared.Master.Customers.Queries;
using static Smlp.Shared.Master.Customers.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Masters;

public class CustomerService : IDisposable
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public CustomerService(IOptions<BackEndOptions> options, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{Customers.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<PaginatedListResponse<GetCustomerResponse>>> GetCustomersAsync(GetCustomerRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<PaginatedListResponse<GetCustomerResponse>>(_navigationManager);
    }

    public async Task<ResponseResult<GetCustomerResponse>> GetCustomerByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<GetCustomerResponse>(_navigationManager);
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonCreateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonUpdateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteCustomerAsync(Guid id)
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
