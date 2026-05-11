using MediatR;
using SecureDocs.Domain.Documents;

namespace SecureDocs.Application.Documents.Commands.SubmitDocument;

public record SubmitDocumentCommand(string Payload) : IRequest<SubmitDocumentResult>;

public record SubmitDocumentResult(Guid DocumentId, DocumentStatus Status);
