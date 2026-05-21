namespace Smlp.Domain.Events;

public class DocumentCreatedEvent : DomainEvent
{
    public DocumentCreatedEvent(Guid documentId)
    {
        DocumentId = documentId;
    }

    public Guid DocumentId { get; }
}
