using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBookingConfiguration : IEntityTypeConfiguration<FlightBookingAggregate>
{
    public void Configure(EntityTypeBuilder<FlightBookingAggregate> builder)
    {
        builder.ToTable("flight_bookings", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_bookings_trip_type",
                "trip_type IN (1, 2)");
            table.HasCheckConstraint(
                "ck_flight_bookings_status",
                "status IN (1, 2, 3)");
            table.HasCheckConstraint(
                "ck_flight_bookings_version_nonnegative",
                "version >= 0");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value));

        builder.Property(x => x.TripType)
            .HasColumnName("trip_type")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .HasDefaultValue(FlightBookingStatus.Pending)
            .IsRequired();

        builder.Property(x => x.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(x => x.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        builder.Ignore(x => x.JourneyCount);
        builder.Ignore(x => x.PassengerCount);
        builder.Ignore(x => x.Outbound);
        builder.Ignore(x => x.ReturnJourney);

        builder.HasMany(x => x.Journeys)
            .WithOne()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Journeys)
            .HasField("_journeys")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Passengers)
            .WithOne()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Passengers)
            .HasField("_passengers")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
