using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.DomainEvent.Models;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Events;
using Smlp.Shared.Documents.Constants;

namespace Smlp.Application.Documents.EventHandlers;

public class DocumentCreatedEventHandler : INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>
{
    private readonly ISmlpDbContext _context;
    private readonly ILogger<DocumentCreatedEventHandler> _logger;

    public DocumentCreatedEventHandler(ISmlpDbContext context, ILogger<DocumentCreatedEventHandler> logger)
    {
        _context = context;
        _logger = logger;
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

        _logger.LogInformation("Document created: {DocumentId} - {FileName}", document.Id, document.FileName);
    }
}
