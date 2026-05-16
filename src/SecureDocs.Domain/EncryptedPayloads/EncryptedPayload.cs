using SecureDocs.Domain.Common;

namespace SecureDocs.Domain.EncryptedPayloads;

public class EncryptedPayload : Entity
{
    private const int Sha256HashLength = 32;
    private const int Ed25519SignatureLength = 64;
    private const int MinimumSaltLength = 16;

    public Guid DocumentId { get; private set; }
    public byte[] Ciphertext { get; private set; } = null!;
    public byte[] Nonce { get; private set; } = null!;
    public byte[] Tag { get; private set; } = null!;
    public byte[] Salt { get; private set; } = null!;
    public string KdfAlgorithm { get; private set; } = null!;
    public string KdfParameters { get; private set; } = null!;
    public byte[] Hash { get; private set; } = null!;
    public byte[] Signature { get; private set; } = null!;
    public string Algorithm { get; private set; } = null!;
    public DateTimeOffset ProcessedAt { get; private set; }

    private EncryptedPayload(
        Guid id,
        Guid documentId,
        byte[] ciphertext,
        byte[] nonce,
        byte[] tag,
        byte[] salt,
        string kdfAlgorithm,
        string kdfParameters,
        byte[] hash,
        byte[] signature,
        string algorithm,
        DateTimeOffset processedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (documentId == Guid.Empty)
            throw new ArgumentException("DocumentId cannot be empty.", nameof(documentId));

        if (ciphertext is null || ciphertext.Length == 0)
            throw new ArgumentException("Ciphertext cannot be null or empty.", nameof(ciphertext));

        if (nonce is null || nonce.Length == 0)
            throw new ArgumentException("Nonce cannot be null or empty.", nameof(nonce));

        if (tag is null || tag.Length == 0)
            throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));

        if (salt is null || salt.Length < MinimumSaltLength)
            throw new ArgumentException(
                $"Salt must be at least {MinimumSaltLength} bytes.",
                nameof(salt));

        if (string.IsNullOrWhiteSpace(kdfAlgorithm))
            throw new ArgumentException("KdfAlgorithm cannot be null or empty.", nameof(kdfAlgorithm));

        if (string.IsNullOrWhiteSpace(kdfParameters))
            throw new ArgumentException("KdfParameters cannot be null or empty.", nameof(kdfParameters));

        if (hash is null || hash.Length != Sha256HashLength)
            throw new ArgumentException(
                $"Hash must be exactly {Sha256HashLength} bytes (SHA-256).",
                nameof(hash));

        if (signature is null || signature.Length != Ed25519SignatureLength)
            throw new ArgumentException(
                $"Signature must be exactly {Ed25519SignatureLength} bytes (Ed25519).",
                nameof(signature));

        if (string.IsNullOrWhiteSpace(algorithm))
            throw new ArgumentException("Algorithm cannot be null or empty.", nameof(algorithm));

        if (processedAt == default)
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(processedAt));

        Id = id;
        DocumentId = documentId;
        Ciphertext = ciphertext;
        Nonce = nonce;
        Tag = tag;
        Salt = salt;
        KdfAlgorithm = kdfAlgorithm;
        KdfParameters = kdfParameters;
        Hash = hash;
        Signature = signature;
        Algorithm = algorithm;
        ProcessedAt = processedAt;
    }

    public static EncryptedPayload Create(
        Guid documentId,
        byte[] ciphertext,
        byte[] nonce,
        byte[] tag,
        byte[] salt,
        string kdfAlgorithm,
        string kdfParameters,
        byte[] hash,
        byte[] signature,
        string algorithm,
        DateTimeOffset processedAt)
    {
        return new EncryptedPayload(
            id: Guid.NewGuid(),
            documentId: documentId,
            ciphertext: ciphertext,
            nonce: nonce,
            tag: tag,
            salt: salt,
            kdfAlgorithm: kdfAlgorithm,
            kdfParameters: kdfParameters,
            hash: hash,
            signature: signature,
            algorithm: algorithm,
            processedAt: processedAt);
    }
}
