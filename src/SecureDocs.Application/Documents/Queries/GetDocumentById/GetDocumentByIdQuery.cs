using MediatR;
using SecureDocs.Domain.Documents;

namespace SecureDocs.Application.Documents.Queries.GetDocumentById;

public record GetDocumentByIdQuery(Guid Id) : IRequest<DocumentDto?>;

public record DocumentDto(Guid Id, DocumentStatus Status, DateTimeOffset SubmittedAt);
