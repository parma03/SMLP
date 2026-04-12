namespace Smlp.Domain.Events;

public class TicketClosedEvent : DomainEvent
{
    public Guid TicketId { get; set; }
    public string Message { get; set; }

    public TicketClosedEvent(Guid ticketId, string message)
    {
        TicketId = ticketId;
        Message = message;
    }
}
