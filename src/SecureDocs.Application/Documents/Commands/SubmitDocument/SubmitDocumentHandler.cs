using MediatR;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Domain.Documents;

namespace SecureDocs.Application.Documents.Commands.SubmitDocument;

public class SubmitDocumentHandler : IRequestHandler<SubmitDocumentCommand, SubmitDocumentResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IPayloadStore _payloadStore;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitDocumentHandler(
        IDocumentRepository documentRepository,
        IPayloadStore payloadStore,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _payloadStore = payloadStore;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmitDocumentResult> Handle(SubmitDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = Document.Submit();

        await _payloadStore.SaveAsync(document.Id, request.Payload, cancellationToken);

        await _documentRepository.AddAsync(document, cancellationToken);

        foreach (var domainEvent in document.DomainEvents)
        {
            await _outboxWriter.AddAsync(domainEvent, cancellationToken);
        }

        document.ClearDomainEvents();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitDocumentResult(document.Id, document.Status);
    }
}
