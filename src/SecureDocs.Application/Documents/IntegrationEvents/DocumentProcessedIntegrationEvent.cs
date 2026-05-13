using SecureDocs.Application.Common.Interfaces;

namespace SecureDocs.Application.Documents.IntegrationEvents;

public record DocumentProcessedIntegrationEvent(
    Guid MessageId,
    Guid DocumentId,
    string Status,
    byte[]? Ciphertext,
    byte[]? Nonce,
    byte[]? Tag,
    byte[]? Hash,
    byte[]? Signature,
    string? Algorithm,
    string? ErrorReason,
    DateTimeOffset ProcessedAt
) : IIntegrationEvent;
