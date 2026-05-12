namespace SecureDocs.Application.Common.Interfaces;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken)
        where T : class, IIntegrationEvent;
}
