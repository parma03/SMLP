namespace Smlp.Domain.Events;

public class TicketSubmittedEvent : DomainEvent
{
    public Guid TicketId { get; set; }

    public TicketSubmittedEvent(Guid ticketId)
    {
        TicketId = ticketId;
    }
}
