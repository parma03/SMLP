namespace Smlp.Domain.Events;

public class TicketCancelledEvent : DomainEvent
{
    public Guid TicketId { get; set; }

    public TicketCancelledEvent(Guid ticketId)
    {
        TicketId = ticketId;
    }
}
