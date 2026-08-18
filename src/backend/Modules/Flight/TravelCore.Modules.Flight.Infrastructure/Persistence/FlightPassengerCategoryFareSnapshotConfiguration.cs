using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightPassengerCategoryFareSnapshotConfiguration
    : IEntityTypeConfiguration<FlightPassengerCategoryFareSnapshot>
{
    public void Configure(EntityTypeBuilder<FlightPassengerCategoryFareSnapshot> builder)
    {
        builder.ToTable("flight_passenger_category_fare_snapshots", table =>
        {
            table.HasCheckConstraint(
                "ck_flight_passenger_category_fare_snapshots_category",
                "category IN (1, 2, 3)");
        });
        builder.HasKey(x => new { x.FlightOfferSnapshotId, x.Ordinal });

        builder.Property(x => x.FlightOfferSnapshotId)
            .HasColumnName("flight_offer_snapshot_id")
            .HasConversion(id => id.Value, value => FlightOfferSnapshotId.From(value));

        builder.Property(x => x.Ordinal)
            .HasColumnName("ordinal")
            .IsRequired();

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.PassengerCount)
            .HasColumnName("passenger_count")
            .IsRequired();

        builder.OwnsRequiredMoney(x => x.Amount, "amount", "currency_code");

        builder.HasOne<FlightBookingMonetarySnapshot>()
            .WithMany(x => x.CategoryFares)
            .HasForeignKey(x => x.FlightOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
