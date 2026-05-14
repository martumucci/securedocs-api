using MassTransit;
using MediatR;
using SecureDocs.Application.Documents.Commands.CompleteDocumentProcessing;
using SecureDocs.Application.Documents.Commands.MarkDocumentAsFailed;
using SecureDocs.Application.Documents.IntegrationEvents;
using LogContext = Serilog.Context.LogContext;

namespace SecureDocs.Infrastructure.Messaging.Consumers;

public class DocumentProcessedConsumer : IConsumer<DocumentProcessedIntegrationEvent>
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private readonly IMediator _mediator;

    public DocumentProcessedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<DocumentProcessedIntegrationEvent> context)
    {
        var correlationId = context.Headers.Get<string>(CorrelationIdHeader)
            ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            var message = context.Message;

            if (message.Status == "Success")
            {
                await _mediator.Send(
                    new CompleteDocumentProcessingCommand(
                        DocumentId: message.DocumentId,
                        Ciphertext: message.Ciphertext!,
                        Nonce: message.Nonce!,
                        Tag: message.Tag!,
                        Salt: message.Salt!,
                        KdfAlgorithm: message.KdfAlgorithm!,
                        KdfParameters: message.KdfParameters!,
                        Hash: message.Hash!,
                        Signature: message.Signature!,
                        Algorithm: message.Algorithm!),
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
}
