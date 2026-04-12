namespace Smlp.Domain.Events;

public class DocumentCreatedEvent : DomainEvent
{
    public Guid DocumentId { get; set; }

    public DocumentCreatedEvent(Guid ticketAttachmentId)
    {
        DocumentId = ticketAttachmentId;
    }
}
