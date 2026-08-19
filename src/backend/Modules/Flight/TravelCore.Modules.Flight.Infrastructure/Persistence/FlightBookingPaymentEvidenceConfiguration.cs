using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBookingPaymentEvidenceConfiguration
    : IEntityTypeConfiguration<FlightBookingPaymentEvidence>
{
    public void Configure(EntityTypeBuilder<FlightBookingPaymentEvidence> builder)
    {
        builder.ToTable("flight_booking_payment_evidence");
        builder.HasKey(x => x.FlightBookingId);

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value));

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasColumnName("currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.VerifiedAt)
            .HasColumnName("verified_at")
            .IsRequired();

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PaymentId)
            .IsUnique()
            .HasDatabaseName("ux_flight_booking_payment_evidence_payment_id");
    }
}
