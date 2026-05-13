using MediatR;

namespace SecureDocs.Application.Documents.Commands.CompleteDocumentProcessing;

public record CompleteDocumentProcessingCommand(
    Guid DocumentId,
    byte[] Ciphertext,
    byte[] Nonce,
    byte[] Tag,
    byte[] Hash,
    byte[] Signature,
    string Algorithm
) : IRequest;
