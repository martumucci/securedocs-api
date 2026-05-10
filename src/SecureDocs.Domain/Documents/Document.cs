using SecureDocs.Domain.Common;
using SecureDocs.Domain.Documents.Events;

namespace SecureDocs.Domain.Documents;

public class Document : Entity
{
    public DocumentStatus Status { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }

    private Document(Guid id, DocumentStatus status, DateTimeOffset submittedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (!Enum.IsDefined(typeof(DocumentStatus), status))
            throw new ArgumentException("Invalid status value.", nameof(status));

        if (submittedAt == default)
            throw new ArgumentException("SubmittedAt cannot be default.", nameof(submittedAt));

        Id = id;
        Status = status;
        SubmittedAt = submittedAt;
    }

    public static Document Submit()
    {
        var document = new Document(
            id: Guid.NewGuid(),
            status: DocumentStatus.Pending,
            submittedAt: DateTimeOffset.UtcNow
        );

        document.RaiseDomainEvent(new DocumentSubmittedEvent(
            DocumentId: document.Id,
            OccurredAt: document.SubmittedAt
        ));

        return document;
    }
}
