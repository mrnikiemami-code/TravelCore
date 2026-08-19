using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBookingPublicIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<FlightBookingPublicIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<FlightBookingPublicIdempotencyRecord> builder)
    {
        builder.ToTable("flight_booking_public_idempotency");
        builder.HasKey(x => x.IdempotencyKey);

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(FlightBookingPublicIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.FlightBookingId)
            .HasDatabaseName("ix_flight_booking_public_idempotency_flight_booking_id");

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
