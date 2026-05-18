using Microsoft.Extensions.Options;
using Psmp.Client.Common.Extensions;
using Psmp.Client.Services.UserInfo;
using Psmp.Shared.Common.Requests;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Documents.Queries.GetDocuments;
using RestSharp;
using static Psmp.Shared.Documents.Constants.ApiEndpoint.V1;

namespace Psmp.Client.Services.BackEnd;

public class DocumentService
{
    private readonly RestClient _restClient;

    public DocumentService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{backEndServiceOptions.Value.BaseUrl}/{Documents.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<CreateDocumentResponse>> CreateDocumentAsync(CreateDocumentRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post) { AlwaysMultipartFormData = true };
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<CreateDocumentResponse>();
    }

    public async Task<ResponseResult<CreateDocumentResponse>> CreateDocumentByTypeAsync(CreateDocumentByTypeRequest request)
    {
        var restRequest = new RestRequest($"{nameof(Documents.RouteTemplateFor.CreatByDocumentType)}", Method.Post);
        restRequest.AddBody(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<CreateDocumentResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteDocumentAsync(Guid documentId)
    {
        var restRequest = new RestRequest(documentId.ToString(), Method.Delete);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<SuccessResponse>();
    }

    public async Task<ResponseResult<PaginatedListResponse<GetDocumentsDocument>>> GetDocumentsAsync(PaginatedListRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<PaginatedListResponse<GetDocumentsDocument>>();
    }

    public async Task<ResponseResult<GetDocumentFileResponse>> GetDocumentFileAsync(Guid documentId)
    {
        var restRequest = new RestRequest($"{nameof(Documents.RouteTemplateFor.Download)}/{documentId}", Method.Get);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<GetDocumentFileResponse>();
    }
    public async Task<ResponseResult<GetDocumentFileResponse>> GetDocumentFileByDocumentTypeAsync(GetDocumentFileRequest request)
    {
        var restRequest = new RestRequest($"{nameof(Documents.RouteTemplateFor.DownloadByDocumentType)}", Method.Get);
        restRequest.AddParameters(request);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<GetDocumentFileResponse>();
    }
    public async Task<ResponseResult<GetDocumentByIdResponse>> GetDocumentByIdAsync(Guid? documentId)
    {
        var restRequest = new RestRequest($"{nameof(Documents.RouteTemplateFor.ById)}/{documentId}", Method.Get);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<GetDocumentByIdResponse>();
    }
}
