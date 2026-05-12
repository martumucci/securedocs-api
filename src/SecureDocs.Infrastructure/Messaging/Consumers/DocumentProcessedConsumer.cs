using MassTransit;
using MediatR;
using SecureDocs.Application.Documents.Commands.MarkDocumentAsFailed;
using SecureDocs.Application.Documents.Commands.MarkDocumentAsProcessed;
using SecureDocs.Application.Documents.IntegrationEvents;

namespace SecureDocs.Infrastructure.Messaging.Consumers;

public class DocumentProcessedConsumer : IConsumer<DocumentProcessedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public DocumentProcessedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<DocumentProcessedIntegrationEvent> context)
    {
        var message = context.Message;

        if (message.Status == "Success")
        {
            await _mediator.Send(
                new MarkDocumentAsProcessedCommand(message.DocumentId),
                context.CancellationToken);
        }
        else if (message.Status == "Failed")
        {
            await _mediator.Send(
                new MarkDocumentAsFailedCommand(message.DocumentId),
                context.CancellationToken);
        }
        else
        {
            throw new InvalidOperationException(
                $"Unknown status '{message.Status}' in DocumentProcessedIntegrationEvent.");
        }
    }
}
