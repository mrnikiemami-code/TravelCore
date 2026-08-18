using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightOfferIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<FlightOfferIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<FlightOfferIdempotencyRecord> builder)
    {
        builder.ToTable("flight_offer_idempotency");
        builder.HasKey(x => new { x.FlightBookingId, x.IdempotencyKey });

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(FlightOfferIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.SnapshotId)
            .HasColumnName("flight_offer_snapshot_id")
            .HasConversion(id => id.Value, value => FlightOfferSnapshotId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
