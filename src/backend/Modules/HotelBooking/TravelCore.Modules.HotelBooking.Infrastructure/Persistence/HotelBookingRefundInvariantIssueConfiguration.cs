using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingRefundInvariantIssueConfiguration
    : IEntityTypeConfiguration<HotelBookingRefundInvariantIssue>
{
    public void Configure(EntityTypeBuilder<HotelBookingRefundInvariantIssue> builder)
    {
        builder.ToTable("hotel_booking_refund_invariant_issues", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_booking_refund_invariant_kind",
                "kind IN (1, 2)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.RefundId).HasColumnName("refund_id").IsRequired();
        builder.Property(x => x.PaymentId).HasColumnName("payment_id").IsRequired();
        builder.Property(x => x.Kind).HasColumnName("kind").HasConversion<short>().IsRequired();
        builder.Property(x => x.DetectedAt).HasColumnName("detected_at").IsRequired();

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RefundId)
            .IsUnique()
            .HasDatabaseName("ux_hotel_booking_refund_invariant_issues_refund_id");
    }
}
