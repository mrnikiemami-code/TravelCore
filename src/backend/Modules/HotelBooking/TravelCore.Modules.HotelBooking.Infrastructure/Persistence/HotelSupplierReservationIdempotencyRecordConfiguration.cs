using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelSupplierReservationIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<HotelSupplierReservationIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<HotelSupplierReservationIdempotencyRecord> builder)
    {
        builder.ToTable("hotel_supplier_reservation_idempotency");
        builder.HasKey(x => new { x.ReservationId, x.IdempotencyKey });

        builder.Property(x => x.ReservationId)
            .HasColumnName("hotel_supplier_reservation_id")
            .HasConversion(id => id.Value, value => HotelSupplierReservationId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(HotelSupplierReservationIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.AttemptId)
            .HasColumnName("hotel_supplier_reservation_attempt_id")
            .HasConversion(id => id.Value, value => HotelSupplierReservationAttemptId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<HotelSupplierReservation>()
            .WithMany()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
