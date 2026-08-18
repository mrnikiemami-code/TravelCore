using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightSupplierReservationIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<FlightSupplierReservationIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<FlightSupplierReservationIdempotencyRecord> builder)
    {
        builder.ToTable("flight_supplier_reservation_idempotency");
        builder.HasKey(x => new { x.ReservationId, x.IdempotencyKey });

        builder.Property(x => x.ReservationId)
            .HasColumnName("flight_supplier_reservation_id")
            .HasConversion(id => id.Value, value => FlightSupplierReservationId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(FlightSupplierReservationIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.AttemptId)
            .HasColumnName("flight_supplier_reservation_attempt_id")
            .HasConversion(id => id.Value, value => FlightSupplierReservationAttemptId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<FlightSupplierReservation>()
            .WithMany()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
