using MediatR;

namespace SecureDocs.Application.Documents.Queries.GetDocumentIntegrity;

public record GetDocumentIntegrityQuery(Guid DocumentId) : IRequest<DocumentIntegrityDto?>;

public record DocumentIntegrityDto(
    Guid DocumentId,
    byte[] Hash,
    byte[] Signature,
    string Algorithm,
    DateTimeOffset ProcessedAt
);
