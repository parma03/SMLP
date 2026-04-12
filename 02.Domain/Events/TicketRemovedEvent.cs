namespace Smlp.Domain.Events;

public class TicketRemovedEvent : DomainEvent
{
    public Guid TicketId { get; set; }
    public string Message { get; set; }

    public TicketRemovedEvent(Guid ticketId, string message)
    {
        TicketId = ticketId;
        Message = message;
    }
}
