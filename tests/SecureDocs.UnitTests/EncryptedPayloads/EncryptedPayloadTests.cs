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
        var salt = TestData.ValidSalt();
        var hash = TestData.ValidHash();
        var signature = TestData.ValidSignature();

        var payload = TestData.AnEncryptedPayload(
            documentId: documentId,
            ciphertext: ciphertext,
            nonce: nonce,
            tag: tag,
            salt: salt,
            hash: hash,
            signature: signature);

        payload.DocumentId.Should().Be(documentId);
        payload.Ciphertext.Should().BeEquivalentTo(ciphertext);
        payload.Nonce.Should().BeEquivalentTo(nonce);
        payload.Tag.Should().BeEquivalentTo(tag);
        payload.Salt.Should().BeEquivalentTo(salt);
        payload.KdfAlgorithm.Should().Be(TestData.ValidKdfAlgorithm);
        payload.KdfParameters.Should().Be(TestData.ValidKdfParameters);
        payload.Hash.Should().BeEquivalentTo(hash);
        payload.Signature.Should().BeEquivalentTo(signature);
        payload.Algorithm.Should().Be(TestData.ValidAlgorithm);
    }

    [Fact]
    public void Create_PreservesTheGivenProcessedAt()
    {
        var processedAt = new DateTimeOffset(2026, 5, 16, 10, 30, 0, TimeSpan.Zero);

        var payload = TestData.AnEncryptedPayload(processedAt: processedAt);

        payload.ProcessedAt.Should().Be(processedAt);
    }

    [Fact]
    public void Create_WithDefaultProcessedAt_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(processedAt: default(DateTimeOffset));

        act.Should().Throw<ArgumentException>().WithParameterName("processedAt");
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
    public void Create_WithSaltShorterThanMinimum_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(salt: new byte[8]);

        act.Should().Throw<ArgumentException>().WithParameterName("salt");
    }

    [Fact]
    public void Create_WithEmptyKdfAlgorithm_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(kdfAlgorithm: string.Empty);

        act.Should().Throw<ArgumentException>().WithParameterName("kdfAlgorithm");
    }

    [Fact]
    public void Create_WithEmptyKdfParameters_ThrowsArgumentException()
    {
        var act = () => TestData.AnEncryptedPayload(kdfParameters: string.Empty);

        act.Should().Throw<ArgumentException>().WithParameterName("kdfParameters");
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
