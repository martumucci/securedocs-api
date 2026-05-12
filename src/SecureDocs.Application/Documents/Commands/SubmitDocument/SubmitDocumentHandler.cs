using MassTransit;
using MediatR;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Application.Documents.IntegrationEvents;
using SecureDocs.Domain.Documents;
using SecureDocs.Domain.Documents.Events;

namespace SecureDocs.Application.Documents.Commands.SubmitDocument;

public class SubmitDocumentHandler : IRequestHandler<SubmitDocumentCommand, SubmitDocumentResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IPayloadStore _payloadStore;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitDocumentHandler(
        IDocumentRepository documentRepository,
        IPayloadStore payloadStore,
        IPublishEndpoint publishEndpoint,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _payloadStore = payloadStore;
        _publishEndpoint = publishEndpoint;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmitDocumentResult> Handle(SubmitDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = Document.Submit();

        await _payloadStore.SaveAsync(document.Id, request.Payload, cancellationToken);

        await _documentRepository.AddAsync(document, cancellationToken);

        foreach (var domainEvent in document.DomainEvents)
        {
            if (domainEvent is DocumentSubmittedEvent submitted)
            {
                var integrationEvent = new DocumentSubmittedIntegrationEvent(
                    MessageId: Guid.NewGuid(),
                    DocumentId: submitted.DocumentId,
                    SubmittedAt: submitted.OccurredAt);

                await _publishEndpoint.Publish(
                    integrationEvent,
                    ctx => ctx.MessageId = integrationEvent.MessageId,
                    cancellationToken);
            }
        }

        document.ClearDomainEvents();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitDocumentResult(document.Id, document.Status);
    }
}
