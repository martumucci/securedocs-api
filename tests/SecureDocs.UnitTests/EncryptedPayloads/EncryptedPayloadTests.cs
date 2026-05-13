using FluentAssertions;
using SecureDocs.UnitTests.Helpers;

namespace SecureDocs.UnitTests.EncryptedPayloads;

public class EncryptedPayloadTests
{
    [Fact]
    public void Create_WithValidValues_GeneratesNonEmptyId()
    {
        var payload = TestData.AnEncryptedPayload();

        payload.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithValidValues_PreservesAllFields()
    {
        var documentId = Guid.NewGuid();
        var ciphertext = TestData.ValidCiphertext();
        var nonce = TestData.ValidNonce();
        var tag = TestData.ValidTag();
        var hash = TestData.ValidHash();
        var signature = TestData.ValidSignature();

        var payload = TestData.AnEncryptedPayload(
            documentId: documentId,
            ciphertext: ciphertext,
            nonce: nonce,
            tag: tag,
            hash: hash,
            signature: signature);

        payload.DocumentId.Should().Be(documentId);
        payload.Ciphertext.Should().BeEquivalentTo(ciphertext);
        payload.Nonce.Should().BeEquivalentTo(nonce);
        payload.Tag.Should().BeEquivalentTo(tag);
        payload.Hash.Should().BeEquivalentTo(hash);
        payload.Signature.Should().BeEquivalentTo(signature);
        payload.Algorithm.Should().Be(TestData.ValidAlgorithm);
    }

    [Fact]
    public void Create_SetsProcessedAtToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;

        var payload = TestData.AnEncryptedPayload();

        var after = DateTimeOffset.UtcNow;
        payload.ProcessedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_WithEmptyDocumentId_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(documentId: Guid.Empty);

        act.Should().Throw<ArgumentException>().WithParameterName("documentId");
    }

    [Fact]
    public void Create_WithEmptyCiphertext_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(ciphertext: []);

        act.Should().Throw<ArgumentException>().WithParameterName("ciphertext");
    }

    [Fact]
    public void Create_WithEmptyNonce_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(nonce: []);

        act.Should().Throw<ArgumentException>().WithParameterName("nonce");
    }

    [Fact]
    public void Create_WithEmptyTag_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(tag: []);

        act.Should().Throw<ArgumentException>().WithParameterName("tag");
    }

    [Fact]
    public void Create_WithHashOfWrongLength_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(hash: new byte[16]);

        act.Should().Throw<ArgumentException>().WithParameterName("hash");
    }

    [Fact]
    public void Create_WithSignatureOfWrongLength_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(signature: new byte[32]);

        act.Should().Throw<ArgumentException>().WithParameterName("signature");
    }

    [Fact]
    public void Create_WithEmptyAlgorithm_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(algorithm: string.Empty);

        act.Should().Throw<ArgumentException>().WithParameterName("algorithm");
    }

    [Fact]
    public void Create_WithWhitespaceAlgorithm_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(algorithm: "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("algorithm");
    }
}
