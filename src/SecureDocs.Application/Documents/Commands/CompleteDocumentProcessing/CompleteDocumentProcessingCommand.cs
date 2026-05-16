using MediatR;

namespace SecureDocs.Application.Documents.Commands.CompleteDocumentProcessing;

public record CompleteDocumentProcessingCommand(
    Guid DocumentId,
    byte[] Ciphertext,
    byte[] Nonce,
    byte[] Tag,
    byte[] Salt,
    string KdfAlgorithm,
    string KdfParameters,
    byte[] Hash,
    byte[] Signature,
    string Algorithm,
    DateTimeOffset ProcessedAt
) : IRequest;
