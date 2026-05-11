using SecureDocs.Domain.Common;

namespace SecureDocs.Application.Common.Interfaces;

public interface IOutboxWriter
{
    Task AddAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
