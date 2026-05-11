using SecureDocs.Domain.Documents;

namespace SecureDocs.Application.Documents;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken cancellationToken);
}
