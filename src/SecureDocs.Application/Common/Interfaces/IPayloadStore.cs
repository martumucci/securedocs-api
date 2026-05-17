namespace SecureDocs.Application.Common.Interfaces;

public record SubmissionPayload(byte[] Payload, string Passphrase);

public interface IPayloadStore
{
    Task SaveAsync(Guid documentId, SubmissionPayload submission, CancellationToken cancellationToken);
}
