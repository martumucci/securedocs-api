using SecureDocs.Application.Documents;
using SecureDocs.Domain.Documents;

namespace SecureDocs.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
    }
}
