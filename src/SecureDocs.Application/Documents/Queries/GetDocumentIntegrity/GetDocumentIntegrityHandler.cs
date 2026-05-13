using MediatR;
using SecureDocs.Application.EncryptedPayloads;

namespace SecureDocs.Application.Documents.Queries.GetDocumentIntegrity;

public class GetDocumentIntegrityHandler : IRequestHandler<GetDocumentIntegrityQuery, DocumentIntegrityDto?>
{
    private readonly IEncryptedPayloadRepository _encryptedPayloadRepository;

    public GetDocumentIntegrityHandler(IEncryptedPayloadRepository encryptedPayloadRepository)
    {
        _encryptedPayloadRepository = encryptedPayloadRepository;
    }

    public async Task<DocumentIntegrityDto?> Handle(GetDocumentIntegrityQuery request, CancellationToken cancellationToken)
    {
        var encryptedPayload = await _encryptedPayloadRepository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);

        if (encryptedPayload is null)
        {
            return null;
        }

        return new DocumentIntegrityDto(
            DocumentId: encryptedPayload.DocumentId,
            Hash: encryptedPayload.Hash,
            Signature: encryptedPayload.Signature,
            Algorithm: encryptedPayload.Algorithm,
            ProcessedAt: encryptedPayload.ProcessedAt);
    }
}
