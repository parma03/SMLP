/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
using Smlp.Domain.Entities;
using Smlp.Domain.Events;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Options;

namespace Smlp.Application.Documents.Commands.CreateDocument;

public class CreateDocumentCommand : CreateDocumentRequest, IRequest<CreateDocumentResponse>
{
}

public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator(IOptions<DocumentOptions> documentOptions)
    {
        Include(new CreateDocumentRequestValidator(documentOptions));
    }
}

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, CreateDocumentResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;

    public CreateDocumentCommandHandler(
        ISmlpDbContext context,
        IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<CreateDocumentResponse> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        var file = memoryStream.ToArray();

        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var extension = Path.GetExtension(request.File.FileName);
        var filename = $"DOC_{timeStamp}{extension}";

        try
        {
            var document = new Document
            {
                Id = Guid.NewGuid(),
                DocumentType = request.DocumentType,
                FileName = filename,
                FileSize = request.File.Length,
                FileContentType = request.File.ContentType,
                StorageFileId = await _storageService.CreateAsync(file, request.DocumentType, filename)
            };

            document.DomainEvents.Add(new DocumentCreatedEvent(document.Id));

            await _context.Documents.AddAsync(document, cancellationToken);
            await _context.SaveChangesAsync(this, cancellationToken);

            return new CreateDocumentResponse
            {
                DocumentId = document.Id
            };
        }
        catch (Exception)
        {
            throw;
        }
    }
}
