using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBookingCancellationIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<FlightBookingCancellationIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<FlightBookingCancellationIdempotencyRecord> builder)
    {
        builder.ToTable("flight_booking_cancellation_idempotency");
        builder.HasKey(x => new { x.FlightBookingId, x.IdempotencyKey });

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(FlightBookingCancellationIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.CancellationId)
            .HasColumnName("flight_booking_cancellation_id")
            .HasConversion(id => id.Value, value => FlightBookingCancellationId.From(value))
            .IsRequired();

        builder.Property(x => x.AttemptId)
            .HasColumnName("flight_supplier_reversal_attempt_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? FlightSupplierReversalAttemptId.From(value.Value) : null);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<FlightBookingCancellation>()
            .WithMany()
            .HasForeignKey(x => x.CancellationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
