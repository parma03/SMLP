namespace Smlp.Domain.Events;

public class TicketAttachmentUpdatedEvent : DomainEvent
{
    public Guid TicketAttachmentId { get; set; }

    public TicketAttachmentUpdatedEvent(Guid ticketAttachmentId)
    {
        TicketAttachmentId = ticketAttachmentId;
    }
}
