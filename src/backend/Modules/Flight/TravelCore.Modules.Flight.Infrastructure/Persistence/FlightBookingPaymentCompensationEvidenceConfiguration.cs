using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBookingPaymentCompensationEvidenceConfiguration
    : IEntityTypeConfiguration<FlightBookingPaymentCompensationEvidence>
{
    public void Configure(EntityTypeBuilder<FlightBookingPaymentCompensationEvidence> builder)
    {
        builder.ToTable("flight_booking_payment_compensation_evidence", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_booking_payment_compensation_reason",
                "reason IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
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

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FlightBookingId)
            .IsUnique()
            .HasDatabaseName("ux_flight_booking_payment_compensation_evidence_flight_booking_id");
    }
}
