using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
using Smlp.Base.Document.Enums;
using Smlp.Shared.Documents.Constants;
using Smlp.Shared.Documents.Queries.GetDocumentFile;

namespace Smlp.Application.Documents.Queries.GetDocumentFile;

public class GetDocumentFileByDocumentTypeQuery : IRequest<GetDocumentFileResponse>
{
    public Guid DocumentId { get; set; }
    public DocumentType DocumentType { get; set; }
}

public class GetDocumentFileByDocumentTypeQueryHandler : IRequestHandler<GetDocumentFileByDocumentTypeQuery, GetDocumentFileResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;

    public GetDocumentFileByDocumentTypeQueryHandler(ISmlpDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<GetDocumentFileResponse> Handle(GetDocumentFileByDocumentTypeQuery request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .Where(x => !x.IsDeleted && x.Id == request.DocumentId)
            .SingleOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            throw new NotFoundException(DisplayTextFor.Document, request.DocumentId);
        }

        var response = new GetDocumentFileResponse
        {
            FileName = document.FileName,
            ContentType = document.FileContentType,
            Content = await _storageService.ReadAsync(document.StorageFileId, request.DocumentType)
        };

        return response;
    }
}
