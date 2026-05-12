namespace SecureDocs.Infrastructure.Messaging;

public class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int PollIntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 50;
}
