using MediatR;
using SecureDocs.Application.Common.Interfaces;

namespace SecureDocs.Application.Documents.Commands.MarkDocumentAsFailed;

public class MarkDocumentAsFailedHandler : IRequestHandler<MarkDocumentAsFailedCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkDocumentAsFailedHandler(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkDocumentAsFailedCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Document {request.DocumentId} not found.");

        document.MarkAsFailed();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
