using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightJourneyConfiguration : IEntityTypeConfiguration<FlightJourney>
{
    public void Configure(EntityTypeBuilder<FlightJourney> builder)
    {
        builder.ToTable("flight_journeys");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightJourneyId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.Ordinal)
            .HasColumnName("ordinal")
            .IsRequired();

        builder.Ignore(x => x.SegmentCount);
        builder.Ignore(x => x.Origin);
        builder.Ignore(x => x.Destination);

        builder.HasIndex(x => new { x.FlightBookingId, x.Ordinal })
            .IsUnique()
            .HasDatabaseName("ux_flight_journeys_booking_ordinal");

        builder.HasMany(x => x.Segments)
            .WithOne()
            .HasForeignKey(x => x.FlightJourneyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Segments)
            .HasField("_segments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
