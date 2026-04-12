namespace Smlp.Domain.Events;

public class TicketAfterResolvedEvent : DomainEvent
{
    public Guid TicketId { get; set; }

    public TicketAfterResolvedEvent(Guid ticketId)
    {
        TicketId = ticketId;
    }
}
