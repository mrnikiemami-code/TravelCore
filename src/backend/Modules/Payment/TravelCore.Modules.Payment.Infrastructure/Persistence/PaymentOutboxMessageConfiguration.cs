using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class PaymentOutboxMessageConfiguration : IEntityTypeConfiguration<PaymentOutboxMessage>
{
    public void Configure(EntityTypeBuilder<PaymentOutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(x => x.MessageType).HasColumnName("message_type").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.HasIndex(x => x.ProcessedAt).HasDatabaseName("ix_payment_outbox_messages_processed_at");
    }
}
