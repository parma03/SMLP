namespace Smlp.Domain.Events;

public class TicketAttachmentDeletedEvent : DomainEvent
{
    public Guid TicketAttachmentId { get; set; }

    public TicketAttachmentDeletedEvent(Guid ticketAttachmentId)
    {
        TicketAttachmentId = ticketAttachmentId;
    }
}
