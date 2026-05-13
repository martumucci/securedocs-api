using SecureDocs.Application.Documents.Commands.CompleteDocumentProcessing;
using SecureDocs.Domain.EncryptedPayloads;

namespace SecureDocs.UnitTests.Helpers;

internal static class TestData
{
    public const string ValidAlgorithm = "AES-256-GCM";

    public static byte[] ValidCiphertext() => [1, 2, 3];
    public static byte[] ValidNonce() => [4, 5, 6];
    public static byte[] ValidTag() => [7, 8, 9];
    public static byte[] ValidHash() => new byte[32];
    public static byte[] ValidSignature() => new byte[64];

    public static EncryptedPayload AnEncryptedPayload(
        Guid? documentId = null,
        byte[]? ciphertext = null,
        byte[]? nonce = null,
        byte[]? tag = null,
        byte[]? hash = null,
        byte[]? signature = null,
        string? algorithm = null)
    {
        return EncryptedPayload.Create(
            documentId: documentId ?? Guid.NewGuid(),
            ciphertext: ciphertext ?? ValidCiphertext(),
            nonce: nonce ?? ValidNonce(),
            tag: tag ?? ValidTag(),
            hash: hash ?? ValidHash(),
            signature: signature ?? ValidSignature(),
            algorithm: algorithm ?? ValidAlgorithm);
    }

    public static CompleteDocumentProcessingCommand ACompleteCommand(
        Guid? documentId = null,
        byte[]? ciphertext = null,
        byte[]? nonce = null,
        byte[]? tag = null,
        byte[]? hash = null,
        byte[]? signature = null,
        string? algorithm = null)
    {
        return new CompleteDocumentProcessingCommand(
            DocumentId: documentId ?? Guid.NewGuid(),
            Ciphertext: ciphertext ?? ValidCiphertext(),
            Nonce: nonce ?? ValidNonce(),
            Tag: tag ?? ValidTag(),
            Hash: hash ?? ValidHash(),
            Signature: signature ?? ValidSignature(),
            Algorithm: algorithm ?? ValidAlgorithm);
    }
}
