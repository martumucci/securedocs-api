namespace SecureDocs.Domain.Documents;

public class Document
{
    public Guid Id { get; private set; }
    public DocumentStatus Status { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }

    public Document(Guid id, DocumentStatus status, DateTimeOffset submittedAt)
    {
        Id = id;
        Status = status;
        SubmittedAt = submittedAt;
    }
}
