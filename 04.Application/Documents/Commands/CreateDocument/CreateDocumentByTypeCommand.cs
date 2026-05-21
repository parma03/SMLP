
using MediatR;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
using Smlp.Domain.Entities;
using Smlp.Domain.Events;
using Smlp.Shared.Documents.Commands.CreateDocument;

namespace Smlp.Application.Documents.Commands.CreateDocument;

public class CreateDocumentByTypeCommand : CreateDocumentByTypeRequest, IRequest<CreateDocumentResponse>
{
}
public class CreateDocumentByTypeCommandHandler : IRequestHandler<CreateDocumentByTypeCommand, CreateDocumentResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;

    public CreateDocumentByTypeCommandHandler(
        ISmlpDbContext context,
        IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<CreateDocumentResponse> Handle(CreateDocumentByTypeCommand request, CancellationToken cancellationToken)
    {
        var readDocument = await _storageService.ReadAsync(request.Filename, request.DocumentType);
        var document = new Document
        {
            Id = Guid.NewGuid(),
            DocumentType = request.DocumentType,
            FileName = request.Filename,
            FileSize = readDocument is not null ? readDocument.Length : 0,
            FileContentType = "application/pdf",
            StorageFileId = request.Filename,
        };

        document.DomainEvents.Add(new DocumentCreatedEvent(document.Id));

        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new CreateDocumentResponse
        {
            DocumentId = document.Id
        };
    }
}
