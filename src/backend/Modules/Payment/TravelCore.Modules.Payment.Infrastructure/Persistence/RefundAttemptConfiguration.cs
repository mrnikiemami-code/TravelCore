using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class RefundAttemptConfiguration : IEntityTypeConfiguration<RefundAttempt>
{
    public void Configure(EntityTypeBuilder<RefundAttempt> builder)
    {
        builder.ToTable("refund_attempts", table =>
        {
            table.HasCheckConstraint("ck_refund_attempts_status", "status IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => RefundAttemptId.From(value));

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.InitiatedAt)
            .HasColumnName("initiated_at");

        builder.Property(x => x.StatusChangedAt)
            .HasColumnName("status_changed_at")
            .IsRequired();

        builder.Property(x => x.ProviderKey)
            .HasColumnName("provider_key")
            .HasMaxLength(ProviderKey.MaxLength)
            .HasConversion(
                key => key.HasValue ? key.Value.Value : null,
                value => string.IsNullOrEmpty(value) ? null : new ProviderKey(value));

        builder.Property(x => x.ProviderRequestReference)
            .HasColumnName("provider_request_reference")
            .HasMaxLength(ProviderRequestReference.MaxLength)
            .HasConversion(
                reference => reference.HasValue ? reference.Value.Value : null,
                value => string.IsNullOrEmpty(value) ? null : new ProviderRequestReference(value));

        builder.Property(x => x.ProviderTransactionReference)
            .HasColumnName("provider_transaction_reference")
            .HasMaxLength(ProviderTransactionReference.MaxLength)
            .HasConversion(
                reference => reference.HasValue ? reference.Value.Value : null,
                value => string.IsNullOrEmpty(value) ? null : new ProviderTransactionReference(value));

        builder.Ignore(x => x.IsTerminal);
        builder.Ignore(x => x.IsActive);

        builder.Property<RefundId>("RefundId")
            .HasColumnName("refund_id")
            .HasConversion(id => id.Value, value => RefundId.From(value));

        builder.HasIndex("RefundId")
            .IsUnique()
            .HasDatabaseName("ux_refund_attempts_one_active_per_refund")
            .HasFilter("status IN (1, 2)");

        builder.HasIndex(x => new { x.ProviderKey, x.ProviderRequestReference })
            .HasDatabaseName("ux_refund_attempts_provider_request")
            .IsUnique()
            .HasFilter("provider_key IS NOT NULL AND provider_request_reference IS NOT NULL");

        builder.HasIndex(x => new { x.ProviderKey, x.ProviderTransactionReference })
            .HasDatabaseName("ux_refund_attempts_provider_transaction")
            .IsUnique()
            .HasFilter("provider_key IS NOT NULL AND provider_transaction_reference IS NOT NULL");
    }
}
