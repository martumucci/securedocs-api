using SecureDocs.Application.Common.Interfaces;
using StackExchange.Redis;

namespace SecureDocs.Infrastructure.Redis;

public class RedisPayloadStore : IPayloadStore
{
    private static readonly TimeSpan PayloadTtl = TimeSpan.FromMinutes(5);

    private readonly IConnectionMultiplexer _multiplexer;

    public RedisPayloadStore(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }

    public async Task SaveAsync(Guid documentId, string payload, CancellationToken cancellationToken)
    {
        var database = _multiplexer.GetDatabase();
        var key = $"payload:{documentId}";

        await database.StringSetAsync(key, payload, PayloadTtl);
    }
}
