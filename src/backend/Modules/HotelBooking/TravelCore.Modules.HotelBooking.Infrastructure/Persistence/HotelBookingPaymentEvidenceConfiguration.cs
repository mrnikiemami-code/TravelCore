using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingPaymentEvidenceConfiguration
    : IEntityTypeConfiguration<HotelBookingPaymentEvidence>
{
    public void Configure(EntityTypeBuilder<HotelBookingPaymentEvidence> builder)
    {
        builder.ToTable("hotel_booking_payment_evidence");
        builder.HasKey(x => x.HotelBookingId);

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value));

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

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PaymentId)
            .IsUnique()
            .HasDatabaseName("ux_hotel_booking_payment_evidence_payment_id");
    }
}
