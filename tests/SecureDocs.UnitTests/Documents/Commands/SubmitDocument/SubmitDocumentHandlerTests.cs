using FluentAssertions;
using NSubstitute;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Application.Documents;
using SecureDocs.Application.Documents.Commands.SubmitDocument;
using SecureDocs.Application.Documents.IntegrationEvents;
using SecureDocs.Domain.Documents;

namespace SecureDocs.UnitTests.Documents.Commands.SubmitDocument;

public class SubmitDocumentHandlerTests
{
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IPayloadStore _payloadStore = Substitute.For<IPayloadStore>();
    private readonly IIntegrationEventPublisher _integrationEventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly SubmitDocumentHandler _handler;

    public SubmitDocumentHandlerTests()
    {
        _handler = new SubmitDocumentHandler(
            _documentRepository,
            _payloadStore,
            _integrationEventPublisher,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidPayload_ReturnsResultWithPendingStatus()
    {
        // Arrange
        var command = new SubmitDocumentCommand("any payload");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(DocumentStatus.Pending);
        result.DocumentId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithValidPayload_SavesPayloadToStore()
    {
        // Arrange
        const string payload = "sensitive content";
        var command = new SubmitDocumentCommand(payload);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _payloadStore.Received(1).SaveAsync(
            Arg.Any<Guid>(),
            payload,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidPayload_PersistsDocumentAndCommitsUnitOfWork()
    {
        // Arrange
        var command = new SubmitDocumentCommand("any payload");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _documentRepository.Received(1).AddAsync(
            Arg.Any<Document>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidPayload_PublishesIntegrationEvent()
    {
        // Arrange
        var command = new SubmitDocumentCommand("any payload");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _integrationEventPublisher.Received(1).PublishAsync(
            Arg.Any<DocumentSubmittedIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidPayload_PublishedEventHasMatchingDocumentId()
    {
        // Arrange
        var command = new SubmitDocumentCommand("any payload");
        Document? capturedDocument = null;
        DocumentSubmittedIntegrationEvent? capturedEvent = null;

        await _documentRepository.AddAsync(
            Arg.Do<Document>(d => capturedDocument = d),
            Arg.Any<CancellationToken>());

        await _integrationEventPublisher.PublishAsync(
            Arg.Do<DocumentSubmittedIntegrationEvent>(e => capturedEvent = e),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedEvent.Should().NotBeNull();
        capturedEvent!.DocumentId.Should().Be(capturedDocument!.Id);
    }
}
