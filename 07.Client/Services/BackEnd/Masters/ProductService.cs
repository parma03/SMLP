using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
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

    public ProductService(IOptions<BackEndOptions> options, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{Products.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetProductResponse>>> GetProductsAsync(GetProductRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<PaginatedListResponse<GetProductResponse>>();
    }

    public async Task<ResponseResult<GetProductResponse>> GetProductByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<GetProductResponse>();
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateProductAsync(CreateProductRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonCreateResponse>();
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonUpdateResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteProductAsync(Guid id)
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
