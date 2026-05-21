using MediatR;

namespace Smlp.Application.Services.DomainEvent.Models;

public class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : Domain.Events.DomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public TDomainEvent DomainEvent { get; }
}
