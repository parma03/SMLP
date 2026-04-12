namespace Smlp.Domain.Events;

public class TicketHandledEvent : DomainEvent
{
    public Guid TicketId { get; set; }

    public TicketHandledEvent(Guid ticketId)
    {
        TicketId = ticketId;
    }
}
