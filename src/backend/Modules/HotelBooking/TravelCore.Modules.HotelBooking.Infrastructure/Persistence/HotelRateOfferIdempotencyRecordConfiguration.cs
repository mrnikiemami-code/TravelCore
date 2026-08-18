using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelRateOfferIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<HotelRateOfferIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<HotelRateOfferIdempotencyRecord> builder)
    {
        builder.ToTable("hotel_rate_offer_idempotency");
        builder.HasKey(x => new { x.HotelBookingId, x.IdempotencyKey });

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(HotelRateOfferIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.SnapshotId)
            .HasColumnName("hotel_rate_offer_snapshot_id")
            .HasConversion(id => id.Value, value => HotelRateOfferSnapshotId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
