namespace Smlp.Domain.Events;

public class TicketAutoClosedEvent : DomainEvent
{
    public Guid TicketId { get; set; }

    public TicketAutoClosedEvent(Guid ticketId)
    {
        TicketId = ticketId;
    }
}
