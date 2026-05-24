using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Common.Handlers;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Requests;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Documents.Queries.GetDocuments;
using RestSharp;
using static Smlp.Shared.Documents.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd;

public class DocumentService
{
    private readonly RestClient _restClient;
    private readonly NavigationManager _navigationManager;

    public DocumentService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo, NavigationManager navigationManager)
    {
        _restClient = new RestClient($"{backEndServiceOptions.Value.BaseUrl}/{Documents.Segment}");
        _restClient.AddUserInfo(userInfo);
        _navigationManager = navigationManager;
    }

    public async Task<ResponseResult<CreateDocumentResponse>> CreateDocumentAsync(CreateDocumentRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post) { AlwaysMultipartFormData = true };
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResultWithRedirect<CreateDocumentResponse>(_navigationManager);
    }

    public async Task<ResponseResult<CreateDocumentResponse>> CreateDocumentByTypeAsync(CreateDocumentByTypeRequest request)
    {
        var restRequest = new RestRequest($"{nameof(Documents.RouteTemplateFor.CreatByDocumentType)}", Method.Post);
        restRequest.AddBody(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResultWithRedirect<CreateDocumentResponse>(_navigationManager);
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteDocumentAsync(Guid documentId)
    {
        var restRequest = new RestRequest(documentId.ToString(), Method.Delete);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResultWithRedirect<SuccessResponse>(_navigationManager);
    }

    public async Task<ResponseResult<PaginatedListResponse<GetDocumentsDocument>>> GetDocumentsAsync(PaginatedListRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResultWithRedirect<PaginatedListResponse<GetDocumentsDocument>>(_navigationManager);
    }

    public async Task<ResponseResult<GetDocumentFileResponse>> GetDocumentFileAsync(Guid documentId)
    {
        var restRequest = new RestRequest($"{nameof(Documents.RouteTemplateFor.Download)}/{documentId}", Method.Get);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResultWithRedirect<GetDocumentFileResponse>(_navigationManager);
    }
    public async Task<ResponseResult<GetDocumentFileResponse>> GetDocumentFileByDocumentTypeAsync(GetDocumentFileRequest request)
    {
        var restRequest = new RestRequest($"{nameof(Documents.RouteTemplateFor.DownloadByDocumentType)}", Method.Get);
        restRequest.AddParameters(request);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResultWithRedirect<GetDocumentFileResponse>(_navigationManager);
    }
    public async Task<ResponseResult<GetDocumentByIdResponse>> GetDocumentByIdAsync(Guid? documentId)
    {
        var restRequest = new RestRequest($"{nameof(Documents.RouteTemplateFor.ById)}/{documentId}", Method.Get);
        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResultWithRedirect<GetDocumentByIdResponse>(_navigationManager);
    }
}
