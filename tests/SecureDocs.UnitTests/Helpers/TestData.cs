using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Application.Documents.Commands.CompleteDocumentProcessing;
using SecureDocs.Domain.EncryptedPayloads;

namespace SecureDocs.UnitTests.Helpers;

internal static class TestData
{
    public const string ValidAlgorithm = "AES-256-GCM";
    public const string ValidKdfAlgorithm = "scrypt";
    public const string ValidKdfParameters = "{\"n\":16384,\"r\":8,\"p\":1}";
    public const string ValidPassphrase = "correct horse battery staple";

    public static readonly DateTimeOffset ValidProcessedAt =
        new(2026, 5, 16, 12, 0, 0, TimeSpan.Zero);

    public static byte[] ValidCiphertext() => [1, 2, 3];
    public static byte[] ValidNonce() => [4, 5, 6];
    public static byte[] ValidTag() => [7, 8, 9];
    public static byte[] ValidSalt() => new byte[16];
    public static byte[] ValidHash() => new byte[32];
    public static byte[] ValidSignature() => new byte[64];

    public static EncryptedPayload AnEncryptedPayload(
        Guid? documentId = null,
        byte[]? ciphertext = null,
        byte[]? nonce = null,
        byte[]? tag = null,
        byte[]? salt = null,
        string? kdfAlgorithm = null,
        string? kdfParameters = null,
        byte[]? hash = null,
        byte[]? signature = null,
        string? algorithm = null,
        DateTimeOffset? processedAt = null)
    {
        return EncryptedPayload.Create(
            documentId: documentId ?? Guid.NewGuid(),
            ciphertext: ciphertext ?? ValidCiphertext(),
            nonce: nonce ?? ValidNonce(),
            tag: tag ?? ValidTag(),
            salt: salt ?? ValidSalt(),
            kdfAlgorithm: kdfAlgorithm ?? ValidKdfAlgorithm,
            kdfParameters: kdfParameters ?? ValidKdfParameters,
            hash: hash ?? ValidHash(),
            signature: signature ?? ValidSignature(),
            algorithm: algorithm ?? ValidAlgorithm,
            processedAt: processedAt ?? ValidProcessedAt);
    }

    public static CompleteDocumentProcessingCommand ACompleteCommand(
        Guid? documentId = null,
        byte[]? ciphertext = null,
        byte[]? nonce = null,
        byte[]? tag = null,
        byte[]? salt = null,
        string? kdfAlgorithm = null,
        string? kdfParameters = null,
        byte[]? hash = null,
        byte[]? signature = null,
        string? algorithm = null,
        DateTimeOffset? processedAt = null)
    {
        return new CompleteDocumentProcessingCommand(
            DocumentId: documentId ?? Guid.NewGuid(),
            Ciphertext: ciphertext ?? ValidCiphertext(),
            Nonce: nonce ?? ValidNonce(),
            Tag: tag ?? ValidTag(),
            Salt: salt ?? ValidSalt(),
            KdfAlgorithm: kdfAlgorithm ?? ValidKdfAlgorithm,
            KdfParameters: kdfParameters ?? ValidKdfParameters,
            Hash: hash ?? ValidHash(),
            Signature: signature ?? ValidSignature(),
            Algorithm: algorithm ?? ValidAlgorithm,
            ProcessedAt: processedAt ?? ValidProcessedAt);
    }

    public static byte[] ValidDocumentBytes() => "any document"u8.ToArray();

    public static SubmissionPayload ASubmissionPayload(
        byte[]? payload = null,
        string? passphrase = null)
    {
        return new SubmissionPayload(
            Payload: payload ?? ValidDocumentBytes(),
            Passphrase: passphrase ?? ValidPassphrase);
    }
}
