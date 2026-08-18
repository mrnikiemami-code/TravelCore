using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class BookingRefundInvariantIssueConfiguration
    : IEntityTypeConfiguration<BookingRefundInvariantIssue>
{
    public void Configure(EntityTypeBuilder<BookingRefundInvariantIssue> builder)
    {
        builder.ToTable("booking_refund_invariant_issues", table =>
        {
            table.HasCheckConstraint("ck_booking_refund_invariant_issues_kind", "kind IN (1)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.BookingId)
            .HasColumnName("booking_id")
            .HasConversion(id => id.Value, value => BookingId.From(value))
            .IsRequired();

        builder.Property(x => x.RefundId)
            .HasColumnName("refund_id")
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .IsRequired();

        builder.Property(x => x.DetectedAt)
            .HasColumnName("detected_at")
            .IsRequired();

        builder.HasOne<Domain.Booking>()
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RefundId)
            .IsUnique()
            .HasDatabaseName("ux_booking_refund_invariant_issues_refund_id");
    }
}
