namespace SecureDocs.Application.Common.Interfaces;

public record SubmissionPayload(string Payload, string Passphrase);

public interface IPayloadStore
{
    Task SaveAsync(Guid documentId, SubmissionPayload submission, CancellationToken cancellationToken);
}
