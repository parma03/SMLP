using Microsoft.AspNetCore.Mvc;
using Smlp.Application.Documents.Commands.CreateDocument;
using Smlp.Application.Documents.Commands.DeleteDocument;
using Smlp.Application.Documents.Queries.GetDocumentFile;
using Smlp.Application.Documents.Queries.GetDocuments;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Constants;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Documents.Queries.GetDocuments;
using Smlp.WebApi.Common.Extensions;

namespace Smlp.WebApi.Areas.V1.Controllers;

[ApiVersion(ApiVersioning.V1.Number)]
public class DocumentsController : ApiControllerBase
{
    [HttpPost]
    [Consumes(ContentTypes.MultipartFormData)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateDocumentResponse>> CreateDocument([FromForm] CreateDocumentCommand command)
    {
        return CreatedAtAction(nameof(CreateDocument), await Mediator.Send(command));
    }

    [HttpPost(ApiEndpoint.V1.Documents.RouteTemplateFor.CreatByDocumentType)]
    [Produces(typeof(CreateDocumentResponse))]
    public async Task<ActionResult<CreateDocumentResponse>> CreateDocumentByType([FromBody] CreateDocumentByTypeCommand command)
    {
        return CreatedAtAction(nameof(CreateDocumentByType), await Mediator.Send(command));
    }

    [HttpDelete(ApiEndpoint.V1.Documents.RouteTemplateFor.DocumentId)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDocument([FromRoute] Guid documentId)
    {
        await Mediator.Send(new DeleteDocumentCommand { DocumentId = documentId });

        return NoContent();
    }

    [HttpGet]
    [Produces(typeof(PaginatedListResponse<GetDocumentsDocument>))]
    public async Task<ActionResult<PaginatedListResponse<GetDocumentsDocument>>> GetDocuments([FromQuery] GetDocumentsQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpGet(ApiEndpoint.V1.Documents.RouteTemplateFor.Download)]
    [Produces(typeof(GetDocumentFileResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDocumentFile([FromRoute] Guid documentId)
    {
        var response = await Mediator.Send(new GetDocumentFileQuery { DocumentId = documentId });

        return response.AsFile();
    }
    [HttpGet(ApiEndpoint.V1.Documents.RouteTemplateFor.DownloadByDocumentType)]
    [Produces(typeof(GetDocumentFileResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDocumentFileByDocumentType(GetDocumentFileRequest request)
    {
        var response = await Mediator.Send(new GetDocumentFileByDocumentTypeQuery { DocumentId = request.DocumentId, DocumentType = request.DocumentType });

        return response.AsFile();
    }

    [HttpGet(ApiEndpoint.V1.Documents.RouteTemplateFor.ById)]
    [Produces(typeof(GetDocumentByIdResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetDocumentByIdResponse>> GetDocumentById([FromRoute] Guid? documentId)
    {
        return await Mediator.Send(new GetDocumentByIdQuery { DocumentId = documentId });
    }
}
