namespace Smlp.Application.Services.DomainEvent;

public interface IDomainEventService
{
    Task Publish(Domain.Events.DomainEvent domainEvent, CancellationToken cancellationToken = default);
}
