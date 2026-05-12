using MediatR;

namespace SecureDocs.Application.Documents.Queries.GetDocumentById;

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
{
    private readonly IDocumentRepository _repository;

    public GetDocumentByIdHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DocumentDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (document is null)
        {
            return null;
        }

        return new DocumentDto(document.Id, document.Status, document.SubmittedAt);
    }
}
