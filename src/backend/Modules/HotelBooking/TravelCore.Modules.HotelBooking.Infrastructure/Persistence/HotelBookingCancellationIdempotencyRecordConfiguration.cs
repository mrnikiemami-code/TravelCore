using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingCancellationIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<HotelBookingCancellationIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<HotelBookingCancellationIdempotencyRecord> builder)
    {
        builder.ToTable("hotel_booking_cancellation_idempotency");
        builder.HasKey(x => new { x.HotelBookingId, x.IdempotencyKey });

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(HotelBookingCancellationIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.CancellationId)
            .HasColumnName("hotel_booking_cancellation_id")
            .HasConversion(id => id.Value, value => HotelBookingCancellationId.From(value))
            .IsRequired();

        builder.Property(x => x.AttemptId)
            .HasColumnName("hotel_supplier_cancellation_attempt_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? HotelSupplierCancellationAttemptId.From(value.Value) : null);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<HotelBookingCancellation>()
            .WithMany()
            .HasForeignKey(x => x.CancellationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
