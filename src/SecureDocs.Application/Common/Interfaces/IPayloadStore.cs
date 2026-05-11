namespace SecureDocs.Application.Common.Interfaces;

public interface IPayloadStore
{
    Task SaveAsync(Guid documentId, string payload, CancellationToken cancellationToken);
}
