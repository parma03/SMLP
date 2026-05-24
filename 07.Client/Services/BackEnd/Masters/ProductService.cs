using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Products.Commands.Create;
using Smlp.Shared.Master.Products.Commands.Update;
using Smlp.Shared.Master.Products.Queries;
using static Smlp.Shared.Master.Products.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Masters;

public class ProductService : IDisposable
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public ProductService(IOptions<BackEndOptions> options, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{Products.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<PaginatedListResponse<GetProductResponse>>> GetProductsAsync(GetProductRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<PaginatedListResponse<GetProductResponse>>(_navigationManager);
    }

    public async Task<ResponseResult<GetProductResponse>> GetProductByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<GetProductResponse>(_navigationManager);
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateProductAsync(CreateProductRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonCreateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResultWithRedirect<CommonUpdateResponse>(_navigationManager);
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteProductAsync(Guid id)
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
