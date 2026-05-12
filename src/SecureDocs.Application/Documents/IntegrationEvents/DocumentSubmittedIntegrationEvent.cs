namespace SecureDocs.Application.Documents.IntegrationEvents;

public record DocumentSubmittedIntegrationEvent(
    Guid MessageId,
    Guid DocumentId,
    DateTimeOffset SubmittedAt
);
