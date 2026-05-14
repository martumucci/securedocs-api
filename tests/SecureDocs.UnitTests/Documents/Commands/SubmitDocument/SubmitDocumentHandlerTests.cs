using FluentAssertions;
using NSubstitute;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Application.Documents;
using SecureDocs.Application.Documents.Commands.SubmitDocument;
using SecureDocs.Application.Documents.IntegrationEvents;
using SecureDocs.Domain.Documents;
using SecureDocs.UnitTests.Helpers;

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

    private static SubmitDocumentCommand ValidCommand(string? payload = null, string? passphrase = null)
    {
        return new SubmitDocumentCommand(
            Payload: payload ?? "any payload",
            Passphrase: passphrase ?? TestData.ValidPassphrase);
    }

    [Fact]
    public async Task Handle_WithValidPayload_ReturnsResultWithPendingStatus()
    {
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.Status.Should().Be(DocumentStatus.Pending);
        result.DocumentId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithValidPayload_SavesPayloadAndPassphraseToStore()
    {
        const string payload = "sensitive content";
        const string passphrase = "correct horse battery staple";
        var command = ValidCommand(payload, passphrase);

        await _handler.Handle(command, CancellationToken.None);

        await _payloadStore.Received(1).SaveAsync(
            Arg.Any<Guid>(),
            Arg.Is<SubmissionPayload>(s => s.Payload == payload && s.Passphrase == passphrase),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidPayload_PersistsDocumentAndCommitsUnitOfWork()
    {
        await _handler.Handle(ValidCommand(), CancellationToken.None);

        await _documentRepository.Received(1).AddAsync(
            Arg.Any<Document>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidPayload_PublishesIntegrationEvent()
    {
        await _handler.Handle(ValidCommand(), CancellationToken.None);

        await _integrationEventPublisher.Received(1).PublishAsync(
            Arg.Any<DocumentSubmittedIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidPayload_PublishedEventHasMatchingDocumentId()
    {
        Document? capturedDocument = null;
        DocumentSubmittedIntegrationEvent? capturedEvent = null;

        await _documentRepository.AddAsync(
            Arg.Do<Document>(d => capturedDocument = d),
            Arg.Any<CancellationToken>());

        await _integrationEventPublisher.PublishAsync(
            Arg.Do<DocumentSubmittedIntegrationEvent>(e => capturedEvent = e),
            Arg.Any<CancellationToken>());

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        capturedDocument.Should().NotBeNull();
        capturedEvent.Should().NotBeNull();
        capturedEvent!.DocumentId.Should().Be(capturedDocument!.Id);
    }
}
