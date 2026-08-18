using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightPassengerConfiguration : IEntityTypeConfiguration<FlightPassenger>
{
    public void Configure(EntityTypeBuilder<FlightPassenger> builder)
    {
        builder.ToTable("flight_passengers", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_passengers_category",
                "category IN (1, 2, 3)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightPassengerId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.Ordinal)
            .HasColumnName("ordinal")
            .IsRequired();

        builder.Property(x => x.GivenName)
            .HasColumnName("given_name")
            .HasMaxLength(FlightPassenger.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.FamilyName)
            .HasColumnName("family_name")
            .HasMaxLength(FlightPassenger.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion<short>()
            .IsRequired();

        builder.HasIndex(x => new { x.FlightBookingId, x.Ordinal })
            .IsUnique()
            .HasDatabaseName("ux_flight_passengers_booking_ordinal");
    }
}
