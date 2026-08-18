using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Persistence;

internal sealed class RefundReconciliationIssueConfiguration
    : IEntityTypeConfiguration<RefundReconciliationIssue>
{
    public void Configure(EntityTypeBuilder<RefundReconciliationIssue> builder)
    {
        builder.ToTable("refund_reconciliation_issues", table =>
        {
            table.HasCheckConstraint("ck_refund_reconciliation_issues_kind", "kind IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.RefundId)
            .HasColumnName("refund_id")
            .HasConversion(id => id.Value, value => RefundId.From(value))
            .IsRequired();

        builder.Property(x => x.AttemptId)
            .HasColumnName("attempt_id")
            .HasConversion(id => id.Value, value => RefundAttemptId.From(value))
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .IsRequired();

        builder.Property(x => x.DetectedAt)
            .HasColumnName("detected_at")
            .IsRequired();

        builder.Property(x => x.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.HasOne<Refund>()
            .WithMany()
            .HasForeignKey(x => x.RefundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RefundId)
            .HasDatabaseName("ix_refund_reconciliation_issues_refund_id");

        builder.HasIndex(x => x.AttemptId)
            .HasDatabaseName("ix_refund_reconciliation_issues_attempt_id");
    }
}
