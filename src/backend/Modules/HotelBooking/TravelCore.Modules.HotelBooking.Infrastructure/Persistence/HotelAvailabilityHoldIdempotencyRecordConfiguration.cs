using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelAvailabilityHoldIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<HotelAvailabilityHoldIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<HotelAvailabilityHoldIdempotencyRecord> builder)
    {
        builder.ToTable("hotel_hold_idempotency");
        builder.HasKey(x => new { x.HotelBookingId, x.IdempotencyKey });

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(HotelAvailabilityHoldIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.HoldId)
            .HasColumnName("hotel_availability_hold_id")
            .HasConversion(id => id.Value, value => HotelAvailabilityHoldId.From(value))
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
