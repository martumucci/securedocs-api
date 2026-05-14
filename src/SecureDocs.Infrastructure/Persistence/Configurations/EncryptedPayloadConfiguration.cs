using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureDocs.Domain.EncryptedPayloads;

namespace SecureDocs.Infrastructure.Persistence.Configurations;

public class EncryptedPayloadConfiguration : IEntityTypeConfiguration<EncryptedPayload>
{
    public void Configure(EntityTypeBuilder<EncryptedPayload> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DocumentId)
            .IsRequired();

        builder.HasIndex(e => e.DocumentId)
            .IsUnique();

        builder.Property(e => e.Ciphertext)
            .IsRequired()
            .HasColumnType("bytea");

        builder.Property(e => e.Nonce)
            .IsRequired()
            .HasColumnType("bytea");

        builder.Property(e => e.Tag)
            .IsRequired()
            .HasColumnType("bytea");

        builder.Property(e => e.Salt)
            .IsRequired()
            .HasColumnType("bytea");

        builder.Property(e => e.KdfAlgorithm)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.KdfParameters)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Hash)
            .IsRequired()
            .HasColumnType("bytea");

        builder.Property(e => e.Signature)
            .IsRequired()
            .HasColumnType("bytea");

        builder.Property(e => e.Algorithm)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ProcessedAt)
            .IsRequired();
    }
}
