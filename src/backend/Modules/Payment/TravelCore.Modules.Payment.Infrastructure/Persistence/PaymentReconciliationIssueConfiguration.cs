using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class PaymentReconciliationIssueConfiguration
    : IEntityTypeConfiguration<PaymentReconciliationIssue>
{
    public void Configure(EntityTypeBuilder<PaymentReconciliationIssue> builder)
    {
        builder.ToTable("payment_reconciliation_issues", table =>
        {
            table.HasCheckConstraint("ck_payment_reconciliation_issues_kind", "kind IN (1, 2)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .HasConversion(id => id.Value, value => PaymentId.From(value))
            .IsRequired();

        builder.Property(x => x.AttemptId)
            .HasColumnName("attempt_id")
            .HasConversion(id => id.Value, value => PaymentAttemptId.From(value))
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .IsRequired();

        builder.Property(x => x.DetectedAt)
            .HasColumnName("detected_at")
            .IsRequired();

        builder.Property(x => x.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.HasOne<PaymentAggregate>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PaymentId)
            .HasDatabaseName("ix_payment_reconciliation_issues_payment_id");

        builder.HasIndex(x => x.AttemptId)
            .HasDatabaseName("ix_payment_reconciliation_issues_attempt_id");
    }
}
