namespace Smlp.Domain.Events;

public class TicketAttachmentCreatedEvent : DomainEvent
{
    public Guid TicketAttachmentId { get; set; }

    public TicketAttachmentCreatedEvent(Guid ticketAttachmentId)
    {
        TicketAttachmentId = ticketAttachmentId;
    }
}
