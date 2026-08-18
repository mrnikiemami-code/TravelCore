using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("payment_attempts", table =>
        {
            table.HasCheckConstraint("ck_payment_attempts_status", "status IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PaymentAttemptId.From(value));

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

        builder.Ignore(x => x.IsTerminal);
        builder.Ignore(x => x.IsActive);

        builder.Property<PaymentId>("PaymentId")
            .HasColumnName("payment_id")
            .HasConversion(id => id.Value, value => PaymentId.From(value));

        builder.HasIndex("PaymentId")
            .HasDatabaseName("ix_payment_attempts_payment_id");
    }
}
