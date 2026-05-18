/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.DomainEvent.Models;
using Smlp.Application.Services.Ecm;
using Smlp.Application.Services.Ecm.Models.UploadContent;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
using Smlp.Domain.Events;
using Smlp.Shared.Documents.Constants;

namespace Smlp.Application.Documents.EventHandlers;

public class DocumentCreatedEventHandler : INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;
    private readonly IEcmService _ecmService;

    public DocumentCreatedEventHandler(
        ISmlpDbContext context,
        IStorageService storageService,
        IEcmService ecmService)
    {
        _context = context;
        _storageService = storageService;
        _ecmService = ecmService;
    }

    public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .Where(x => !x.IsDeleted && x.Id == notification.DomainEvent.DocumentId)
            .SingleOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            throw new NotFoundException(DisplayTextFor.Document, notification.DomainEvent.DocumentId);
        }

        var uploadContentModel = new UploadContentRequest
        {
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            FileContentType = document.FileContentType,
            FileContent = await _storageService.ReadAsync(document.StorageFileId, document.DocumentType)
        };

        var uploadContentResult = await _ecmService.UploadContentAsync(uploadContentModel, cancellationToken);
    }
}
