using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.Transactions.Commands.Create;
using Smlp.Shared.Transaction.Transactions.Commands.Update;
using Smlp.Shared.Transaction.Transactions.Queries;
using TransactionEndpoint = Smlp.Shared.Transaction.Transactions.Constants.ApiEndpoint.V1.Transactions;

namespace Smlp.Client.Services.BackEnd.Transactions;

public class TransactionService : IDisposable
{
    private readonly RestClient _restClient;

    public TransactionService(IOptions<BackEndOptions> options, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{TransactionEndpoint.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetTransactionResponse>>> GetTransactionsAsync(GetTransactionRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<PaginatedListResponse<GetTransactionResponse>>();
    }

    public async Task<ResponseResult<GetTransactionResponse>> GetTransactionByIdAsync(Guid id)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Get);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<GetTransactionResponse>();
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonCreateResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> CancelTransactionAsync(Guid id)
    {
        var restRequest = new RestRequest(TransactionEndpoint.RouteTemplateFor.Cancel.Replace("{id:guid}", id.ToString()), Method.Post);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<SuccessResponse>();
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdateTransactionAsync(Guid id, UpdateTransactionRequest request)
    {
        var restRequest = new RestRequest(id.ToString(), Method.Put);
        restRequest.AddJsonBody(request);
        return (await _restClient.ExecuteAsync(restRequest)).ToResponseResult<CommonUpdateResponse>();
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
