using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class PaymentInitiationIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<PaymentInitiationIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<PaymentInitiationIdempotencyRecord> builder)
    {
        builder.ToTable("payment_initiation_idempotency");
        builder.HasKey(x => new { x.PaymentId, x.IdempotencyKey });

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .HasConversion(id => id.Value, value => PaymentId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(PaymentInitiationIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.AttemptId)
            .HasColumnName("attempt_id")
            .HasConversion(id => id.Value, value => PaymentAttemptId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<PaymentAggregate>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AttemptId)
            .HasDatabaseName("ix_payment_initiation_idempotency_attempt_id");
    }
}
