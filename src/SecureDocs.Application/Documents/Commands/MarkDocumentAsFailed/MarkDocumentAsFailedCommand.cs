using MediatR;

namespace SecureDocs.Application.Documents.Commands.MarkDocumentAsFailed;

public record MarkDocumentAsFailedCommand(Guid DocumentId) : IRequest;
