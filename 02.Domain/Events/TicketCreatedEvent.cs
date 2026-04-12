namespace Smlp.Domain.Events;

public class TicketCreatedEvent : DomainEvent
{
    public Guid TicketId { get; set; }

    public TicketCreatedEvent(Guid ticketId)
    {
        TicketId = ticketId;
    }
}
