namespace SecureDocs.Infrastructure.Messaging;

public class MassTransitOptions
{
    public const string SectionName = "MassTransit";

    public MassTransitOutboxSettings Outbox { get; init; } = new();
}

public class MassTransitOutboxSettings
{
    public int QueryDelaySeconds { get; init; } = 5;
    public int QueryMessageLimit { get; init; } = 50;
}
