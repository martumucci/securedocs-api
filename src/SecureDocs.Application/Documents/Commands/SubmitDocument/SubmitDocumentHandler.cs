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
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitDocumentHandler(
        IDocumentRepository documentRepository,
        IPayloadStore payloadStore,
        IIntegrationEventPublisher integrationEventPublisher,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _payloadStore = payloadStore;
        _integrationEventPublisher = integrationEventPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmitDocumentResult> Handle(SubmitDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = Document.Submit();

        var submission = new SubmissionPayload(request.Payload, request.Passphrase);
        await _payloadStore.SaveAsync(document.Id, submission, cancellationToken);

        await _documentRepository.AddAsync(document, cancellationToken);

        foreach (var domainEvent in document.DomainEvents)
        {
            if (domainEvent is DocumentSubmittedEvent submitted)
            {
                var integrationEvent = new DocumentSubmittedIntegrationEvent(
                    MessageId: Guid.NewGuid(),
                    DocumentId: submitted.DocumentId,
                    SubmittedAt: submitted.OccurredAt);

                await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);
            }
        }

        document.ClearDomainEvents();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitDocumentResult(document.Id, document.Status);
    }
}
