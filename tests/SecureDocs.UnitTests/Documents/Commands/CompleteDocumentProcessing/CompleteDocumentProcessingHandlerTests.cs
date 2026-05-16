using FluentAssertions;
using NSubstitute;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Application.Documents;
using SecureDocs.Application.Documents.Commands.CompleteDocumentProcessing;
using SecureDocs.Application.EncryptedPayloads;
using SecureDocs.Domain.Documents;
using SecureDocs.Domain.EncryptedPayloads;
using SecureDocs.UnitTests.Helpers;

namespace SecureDocs.UnitTests.Documents.Commands.CompleteDocumentProcessing;

public class CompleteDocumentProcessingHandlerTests
{
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IEncryptedPayloadRepository _encryptedPayloadRepository = Substitute.For<IEncryptedPayloadRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly CompleteDocumentProcessingHandler _handler;

    public CompleteDocumentProcessingHandlerTests()
    {
        _handler = new CompleteDocumentProcessingHandler(
            _documentRepository,
            _encryptedPayloadRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_PersistsEncryptedPayloadAndMarksDocumentAsProcessed()
    {
        // Arrange
        var document = Document.Submit();
        var command = TestData.ACompleteCommand(documentId: document.Id);
        _documentRepository.GetByIdAsync(document.Id, Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _encryptedPayloadRepository.Received(1).AddAsync(
            Arg.Any<EncryptedPayload>(),
            Arg.Any<CancellationToken>());
        document.Status.Should().Be(DocumentStatus.Processed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDocumentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = TestData.ACompleteCommand();
        _documentRepository.GetByIdAsync(command.DocumentId, Arg.Any<CancellationToken>())
            .Returns((Document?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDocumentAlreadyFailed_ThrowsAndDoesNotCommit()
    {
        // Arrange — a document that already transitioned to Failed cannot be processed
        var document = Document.Submit();
        document.MarkAsFailed();
        var command = TestData.ACompleteCommand(documentId: document.Id);
        _documentRepository.GetByIdAsync(document.Id, Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PersistedEncryptedPayloadCarriesTheCommandFields()
    {
        // Arrange
        var document = Document.Submit();
        var command = TestData.ACompleteCommand(documentId: document.Id);
        _documentRepository.GetByIdAsync(document.Id, Arg.Any<CancellationToken>())
            .Returns(document);

        EncryptedPayload? capturedPayload = null;
        await _encryptedPayloadRepository.AddAsync(
            Arg.Do<EncryptedPayload>(p => capturedPayload = p),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPayload.Should().NotBeNull();
        capturedPayload!.DocumentId.Should().Be(command.DocumentId);
        capturedPayload.Ciphertext.Should().BeEquivalentTo(command.Ciphertext);
        capturedPayload.Nonce.Should().BeEquivalentTo(command.Nonce);
        capturedPayload.Tag.Should().BeEquivalentTo(command.Tag);
        capturedPayload.Salt.Should().BeEquivalentTo(command.Salt);
        capturedPayload.KdfAlgorithm.Should().Be(command.KdfAlgorithm);
        capturedPayload.KdfParameters.Should().Be(command.KdfParameters);
        capturedPayload.Hash.Should().BeEquivalentTo(command.Hash);
        capturedPayload.Signature.Should().BeEquivalentTo(command.Signature);
        capturedPayload.Algorithm.Should().Be(command.Algorithm);
        capturedPayload.ProcessedAt.Should().Be(command.ProcessedAt);
    }
}
