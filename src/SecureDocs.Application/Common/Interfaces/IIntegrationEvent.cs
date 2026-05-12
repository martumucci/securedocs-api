namespace SecureDocs.Application.Common.Interfaces;

public interface IIntegrationEvent
{
    Guid MessageId { get; }
}
