using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Attributes;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
using Smlp.Shared.Documents.Constants;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.Application.Documents.Queries.GetDocumentFile;

[Authorize(Policy = Permissions.Smlp_Document_Download)]
public class GetDocumentFileQuery : IRequest<GetDocumentFileResponse>
{
    public Guid DocumentId { get; set; }
}

public class GetDocumentFileHandler : IRequestHandler<GetDocumentFileQuery, GetDocumentFileResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;

    public GetDocumentFileHandler(ISmlpDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<GetDocumentFileResponse> Handle(GetDocumentFileQuery request, CancellationToken cancellationToken)
    {
        var ticketAttachment = await _context.Documents
            .Where(x => !x.IsDeleted && x.Id == request.DocumentId)
            .SingleOrDefaultAsync(cancellationToken);

        if (ticketAttachment is null)
        {
            throw new NotFoundException(DisplayTextFor.Document, request.DocumentId);
        }

        var response = new GetDocumentFileResponse
        {
            FileName = $"{ticketAttachment.FileName}",
            ContentType = ticketAttachment.FileContentType,
            Content = await _storageService.ReadAsync(ticketAttachment.StorageFileId)
        };

        return response;
    }
}
