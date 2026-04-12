namespace Smlp.Domain.Events;

public class TicketUpdatedEvent : DomainEvent
{
    public Guid TicketId { get; set; }

    public TicketUpdatedEvent(Guid ticketId)
    {
        TicketId = ticketId;
    }
}
