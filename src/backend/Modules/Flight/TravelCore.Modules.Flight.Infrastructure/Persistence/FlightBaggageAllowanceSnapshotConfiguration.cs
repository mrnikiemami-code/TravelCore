using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBaggageAllowanceSnapshotConfiguration
    : IEntityTypeConfiguration<FlightBaggageAllowanceSnapshot>
{
    public void Configure(EntityTypeBuilder<FlightBaggageAllowanceSnapshot> builder)
    {
        builder.ToTable("flight_baggage_allowance_snapshots");
        builder.HasKey(x => new { x.FlightOfferSnapshotId, x.Ordinal });

        builder.Property(x => x.FlightOfferSnapshotId)
            .HasColumnName("flight_offer_snapshot_id")
            .HasConversion(id => id.Value, value => FlightOfferSnapshotId.From(value));

        builder.Property(x => x.Ordinal)
            .HasColumnName("ordinal")
            .IsRequired();

        builder.Property(x => x.Quantity).HasColumnName("quantity");
        builder.Property(x => x.Weight)
            .HasColumnName("weight")
            .HasColumnType("numeric(24,8)");

        builder.Property(x => x.Unit)
            .HasColumnName("unit")
            .HasMaxLength(FlightBaggageAllowanceSnapshot.UnitMaxLength);

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasMaxLength(FlightBaggageAllowanceSnapshot.CategoryMaxLength);

        builder.Property(x => x.PassengerCategory)
            .HasColumnName("passenger_category")
            .HasConversion<short?>();

        builder.HasOne<FlightFareRulesSnapshot>()
            .WithMany(x => x.Baggage)
            .HasForeignKey(x => x.FlightOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
