using FluentAssertions;
using NSubstitute;
using SecureDocs.Application.Documents.Queries.GetDocumentIntegrity;
using SecureDocs.Application.EncryptedPayloads;
using SecureDocs.Domain.EncryptedPayloads;
using SecureDocs.UnitTests.Helpers;

namespace SecureDocs.UnitTests.Documents.Queries.GetDocumentIntegrity;

public class GetDocumentIntegrityHandlerTests
{
    private readonly IEncryptedPayloadRepository _repository = Substitute.For<IEncryptedPayloadRepository>();
    private readonly GetDocumentIntegrityHandler _handler;

    public GetDocumentIntegrityHandlerTests()
    {
        _handler = new GetDocumentIntegrityHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenEncryptedPayloadExists_ReturnsDtoWithCorrectValues()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var payload = TestData.AnEncryptedPayload(documentId: documentId);

        _repository.GetByDocumentIdAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(payload);

        // Act
        var result = await _handler.Handle(
            new GetDocumentIntegrityQuery(documentId),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DocumentId.Should().Be(documentId);
        result.Hash.Should().BeEquivalentTo(payload.Hash);
        result.Signature.Should().BeEquivalentTo(payload.Signature);
        result.Algorithm.Should().Be(payload.Algorithm);
        result.ProcessedAt.Should().Be(payload.ProcessedAt);
    }

    [Fact]
    public async Task Handle_WhenEncryptedPayloadDoesNotExist_ReturnsNull()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _repository.GetByDocumentIdAsync(documentId, Arg.Any<CancellationToken>())
            .Returns((EncryptedPayload?)null);

        // Act
        var result = await _handler.Handle(
            new GetDocumentIntegrityQuery(documentId),
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
