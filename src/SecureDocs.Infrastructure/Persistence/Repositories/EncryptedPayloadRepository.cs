using Microsoft.EntityFrameworkCore;
using SecureDocs.Application.EncryptedPayloads;
using SecureDocs.Domain.EncryptedPayloads;

namespace SecureDocs.Infrastructure.Persistence.Repositories;

public class EncryptedPayloadRepository : IEncryptedPayloadRepository
{
    private readonly ApplicationDbContext _context;

    public EncryptedPayloadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EncryptedPayload encryptedPayload, CancellationToken cancellationToken)
    {
        await _context.EncryptedPayloads.AddAsync(encryptedPayload, cancellationToken);
    }

    public async Task<EncryptedPayload?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return await _context.EncryptedPayloads
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.DocumentId == documentId, cancellationToken);
    }
}
