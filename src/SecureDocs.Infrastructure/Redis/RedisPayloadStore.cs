using System.Text.Json;
using SecureDocs.Application.Common.Interfaces;
using StackExchange.Redis;

namespace SecureDocs.Infrastructure.Redis;

public class RedisPayloadStore : IPayloadStore
{
    private static readonly TimeSpan PayloadTtl = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IConnectionMultiplexer _multiplexer;

    public RedisPayloadStore(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }

    public async Task SaveAsync(Guid documentId, SubmissionPayload submission, CancellationToken cancellationToken)
    {
        var database = _multiplexer.GetDatabase();
        var key = $"payload:{documentId}";
        var json = JsonSerializer.Serialize(submission, SerializerOptions);

        await database.StringSetAsync(key, json, PayloadTtl);
    }
}
