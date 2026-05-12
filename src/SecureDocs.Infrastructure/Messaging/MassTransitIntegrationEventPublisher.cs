using MassTransit;
using SecureDocs.Application.Common.Interfaces;

namespace SecureDocs.Infrastructure.Messaging;

public class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken)
        where T : class, IIntegrationEvent
    {
        return _publishEndpoint.Publish(
            integrationEvent,
            ctx => ctx.MessageId = integrationEvent.MessageId,
            cancellationToken);
    }
}
