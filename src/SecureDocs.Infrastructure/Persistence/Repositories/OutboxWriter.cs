using System.Text.Json;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Domain.Common;

namespace SecureDocs.Infrastructure.Persistence.Repositories;

public class OutboxWriter : IOutboxWriter
{
    private readonly ApplicationDbContext _context;

    public OutboxWriter(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var eventType = domainEvent.GetType();
        var payload = JsonSerializer.Serialize(domainEvent, eventType);

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType.Name,
            Payload = payload,
            OccurredAt = DateTimeOffset.UtcNow,
            ProcessedAt = null
        };

        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
