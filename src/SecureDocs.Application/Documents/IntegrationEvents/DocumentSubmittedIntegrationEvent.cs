using SecureDocs.Application.Common.Interfaces;

namespace SecureDocs.Application.Documents.IntegrationEvents;

public record DocumentSubmittedIntegrationEvent(
    Guid MessageId,
    Guid DocumentId,
    DateTimeOffset SubmittedAt
) : IIntegrationEvent;
