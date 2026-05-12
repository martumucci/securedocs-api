using MassTransit;
using Microsoft.AspNetCore.Http;
using SecureDocs.Application.Common.Interfaces;

namespace SecureDocs.Infrastructure.Messaging;

public class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private const string CorrelationIdHttpContextKey = "CorrelationId";

    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MassTransitIntegrationEventPublisher(
        IPublishEndpoint publishEndpoint,
        IHttpContextAccessor httpContextAccessor)
    {
        _publishEndpoint = publishEndpoint;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken)
        where T : class, IIntegrationEvent
    {
        var correlationId = GetCorrelationId();

        return _publishEndpoint.Publish(
            integrationEvent,
            ctx =>
            {
                ctx.MessageId = integrationEvent.MessageId;

                if (correlationId is not null)
                {
                    ctx.Headers.Set(CorrelationIdHeader, correlationId);
                }
            },
            cancellationToken);
    }

    private string? GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Items.TryGetValue(CorrelationIdHttpContextKey, out var value)
            && value is string correlationId)
        {
            return correlationId;
        }

        return null;
    }
}
