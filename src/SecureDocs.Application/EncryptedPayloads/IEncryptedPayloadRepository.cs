using SecureDocs.Domain.EncryptedPayloads;

namespace SecureDocs.Application.EncryptedPayloads;

public interface IEncryptedPayloadRepository
{
    Task AddAsync(EncryptedPayload encryptedPayload, CancellationToken cancellationToken);

    Task<EncryptedPayload?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken);
}
