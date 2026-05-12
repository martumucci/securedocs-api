using MediatR;

namespace SecureDocs.Application.Documents.Commands.MarkDocumentAsProcessed;

public record MarkDocumentAsProcessedCommand(Guid DocumentId) : IRequest;
