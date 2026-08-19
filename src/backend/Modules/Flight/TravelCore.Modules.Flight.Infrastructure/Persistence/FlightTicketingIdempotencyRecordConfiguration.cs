using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightTicketingIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<FlightTicketingIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<FlightTicketingIdempotencyRecord> builder)
    {
        builder.ToTable("flight_ticketing_idempotency");
        builder.HasKey(x => new { x.FlightBookingId, x.IdempotencyKey });

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value));

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(FlightTicketingIdempotencyRecord.KeyMaxLength)
            .IsRequired();

        builder.Property(x => x.AttemptId)
            .HasColumnName("flight_ticketing_attempt_id")
            .HasConversion(id => id.Value, value => FlightTicketingAttemptId.From(value))
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
