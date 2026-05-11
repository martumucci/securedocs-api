using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SecureDocs.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.EventType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Payload)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(o => o.OccurredAt)
            .IsRequired();

        builder.HasIndex(o => o.ProcessedAt)
            .HasFilter("\"ProcessedAt\" IS NULL");
    }
}
