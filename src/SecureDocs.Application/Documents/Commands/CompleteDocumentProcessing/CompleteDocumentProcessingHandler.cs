using MediatR;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Application.Documents;
using SecureDocs.Application.EncryptedPayloads;
using SecureDocs.Domain.EncryptedPayloads;

namespace SecureDocs.Application.Documents.Commands.CompleteDocumentProcessing;

public class CompleteDocumentProcessingHandler : IRequestHandler<CompleteDocumentProcessingCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IEncryptedPayloadRepository _encryptedPayloadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteDocumentProcessingHandler(
        IDocumentRepository documentRepository,
        IEncryptedPayloadRepository encryptedPayloadRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _encryptedPayloadRepository = encryptedPayloadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompleteDocumentProcessingCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Document {request.DocumentId} not found.");

        var encryptedPayload = EncryptedPayload.Create(
            documentId: request.DocumentId,
            ciphertext: request.Ciphertext,
            nonce: request.Nonce,
            tag: request.Tag,
            hash: request.Hash,
            signature: request.Signature,
            algorithm: request.Algorithm);

        await _encryptedPayloadRepository.AddAsync(encryptedPayload, cancellationToken);

        document.MarkAsProcessed();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
