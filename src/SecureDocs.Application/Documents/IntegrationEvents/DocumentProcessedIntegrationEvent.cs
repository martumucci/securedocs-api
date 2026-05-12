using SecureDocs.Application.Common.Interfaces;

namespace SecureDocs.Application.Documents.IntegrationEvents;

public record DocumentProcessedIntegrationEvent(
    Guid MessageId,
    Guid DocumentId,
    string Status,
    Guid? EncryptedPayloadId,
    string? ErrorReason,
    DateTimeOffset ProcessedAt
) : IIntegrationEvent;
