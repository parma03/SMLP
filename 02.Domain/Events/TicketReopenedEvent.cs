namespace Smlp.Domain.Events;

public class TicketReopenedEvent : DomainEvent
{
    public Guid TicketId { get; set; }
    public string Message { get; set; }

    public TicketReopenedEvent(Guid ticketId, string message)
    {
        TicketId = ticketId;
        Message = message;
    }
}
