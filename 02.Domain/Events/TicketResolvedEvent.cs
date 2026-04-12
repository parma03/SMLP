namespace Smlp.Domain.Events;

public class TicketResolvedEvent : DomainEvent
{
    public Guid TicketId { get; set; }
    public string Message { get; set; }

    public TicketResolvedEvent(Guid ticketId, string message)
    {
        TicketId = ticketId;
        Message = message;
    }
}
