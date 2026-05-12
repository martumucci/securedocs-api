using MediatR;
using SecureDocs.Application.Common.Interfaces;

namespace SecureDocs.Application.Documents.Commands.MarkDocumentAsProcessed;

public class MarkDocumentAsProcessedHandler : IRequestHandler<MarkDocumentAsProcessedCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkDocumentAsProcessedHandler(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkDocumentAsProcessedCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Document {request.DocumentId} not found.");

        document.MarkAsProcessed();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
