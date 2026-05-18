using MediatR;
using Microsoft.Extensions.Logging;
using Psmp.Application.Services.DomainEvent;
using Psmp.Application.Services.DomainEvent.Models;

namespace Psmp.InfrastructureDomainEvent;

public class DomainEventService : IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger;
    private readonly IPublisher _mediator;

    public DomainEventService(ILogger<DomainEventService> logger, IPublisher mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Publish(Domain.Events.DomainEvent domainEvent)
    {
        _logger.LogInformation("Publishing Domain Event: {EventName}", domainEvent.GetType().Name);

        await _mediator.Publish(GetNotificationCorrespondingToDomainEvent(domainEvent));
    }

    private static INotification GetNotificationCorrespondingToDomainEvent(Domain.Events.DomainEvent domainEvent)
    {
        var notification = Activator.CreateInstance(typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()), domainEvent);

        if (notification is null)
        {
            throw new Exception("notification is null");
        }

        return (INotification)notification;
    }
}
