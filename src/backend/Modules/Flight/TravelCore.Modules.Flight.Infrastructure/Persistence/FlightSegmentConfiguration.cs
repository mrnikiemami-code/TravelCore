using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightSegmentConfiguration : IEntityTypeConfiguration<FlightSegment>
{
    public void Configure(EntityTypeBuilder<FlightSegment> builder)
    {
        builder.ToTable("flight_segments", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_segments_arrival_after_departure",
                "arrival_at > departure_at");
            table.HasCheckConstraint(
                "ck_flight_segments_origin_destination_differ",
                "origin_airport_iata <> destination_airport_iata");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightSegmentId.From(value));

        builder.Property(x => x.FlightJourneyId)
            .HasColumnName("flight_journey_id")
            .HasConversion(id => id.Value, value => FlightJourneyId.From(value))
            .IsRequired();

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.Ordinal)
            .HasColumnName("ordinal")
            .IsRequired();

        builder.Property(x => x.Origin)
            .HasColumnName("origin_airport_iata")
            .HasMaxLength(AirportReference.IataCodeLength)
            .HasConversion(reference => reference.IataCode, value => new AirportReference(value))
            .IsRequired();

        builder.Property(x => x.Destination)
            .HasColumnName("destination_airport_iata")
            .HasMaxLength(AirportReference.IataCodeLength)
            .HasConversion(reference => reference.IataCode, value => new AirportReference(value))
            .IsRequired();

        builder.Property(x => x.DepartureAt)
            .HasColumnName("departure_at")
            .IsRequired();

        builder.Property(x => x.DepartureTimeZoneId)
            .HasColumnName("departure_time_zone_id")
            .HasMaxLength(FlightTimeZone.IdMaxLength)
            .IsRequired();

        builder.Property(x => x.ArrivalAt)
            .HasColumnName("arrival_at")
            .IsRequired();

        builder.Property(x => x.ArrivalTimeZoneId)
            .HasColumnName("arrival_time_zone_id")
            .HasMaxLength(FlightTimeZone.IdMaxLength)
            .IsRequired();

        builder.Property(x => x.MarketingCarrier)
            .HasColumnName("marketing_carrier_iata")
            .HasMaxLength(AirlineReference.IataCodeLength)
            .HasConversion(reference => reference.IataCode, value => new AirlineReference(value))
            .IsRequired();

        builder.Property(x => x.OperatingCarrier)
            .HasColumnName("operating_carrier_iata")
            .HasMaxLength(AirlineReference.IataCodeLength)
            .HasConversion(
                reference => reference.HasValue ? reference.Value.IataCode : null,
                value => value == null ? null : new AirlineReference(value));

        builder.Property(x => x.FlightNumber)
            .HasColumnName("flight_number")
            .HasMaxLength(FlightSegment.FlightNumberMaxLength);

        builder.HasIndex(x => new { x.FlightJourneyId, x.Ordinal })
            .IsUnique()
            .HasDatabaseName("ux_flight_segments_journey_ordinal");
    }
}
