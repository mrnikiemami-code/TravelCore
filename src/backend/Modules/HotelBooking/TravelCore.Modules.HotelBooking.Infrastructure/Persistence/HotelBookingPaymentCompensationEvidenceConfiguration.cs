using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingPaymentCompensationEvidenceConfiguration
    : IEntityTypeConfiguration<HotelBookingPaymentCompensationEvidence>
{
    public void Configure(EntityTypeBuilder<HotelBookingPaymentCompensationEvidence> builder)
    {
        builder.ToTable("hotel_booking_payment_compensation_evidence", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_booking_payment_compensation_reason",
                "reason IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.DetectedAt)
            .HasColumnName("detected_at")
            .IsRequired();

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.HotelBookingId)
            .IsUnique()
            .HasDatabaseName("ux_hotel_booking_payment_compensation_evidence_hotel_booking_id");
    }
}
