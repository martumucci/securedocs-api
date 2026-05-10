using SecureDocs.Domain.Common;

namespace SecureDocs.Domain.Documents.Events;

public record DocumentSubmittedEvent(
    Guid DocumentId,
    DateTimeOffset OccurredAt
) : IDomainEvent;
