using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
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

    public CustomerService(IOptions<BackEndOptions> options, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{Customers.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetCustomerResponse>>> GetCustomersAsync(GetCustomerRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<PaginatedListResponse<GetCustomerResponse>>();
    }

    public async Task<ResponseResult<GetCustomerResponse>> GetCustomerByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<GetCustomerResponse>();
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonCreateResponse>();
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonUpdateResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteCustomerAsync(Guid id)
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
