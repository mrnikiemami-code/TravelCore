using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBookingMonetarySnapshotConfiguration
    : IEntityTypeConfiguration<FlightBookingMonetarySnapshot>
{
    public void Configure(EntityTypeBuilder<FlightBookingMonetarySnapshot> builder)
    {
        builder.ToTable("flight_booking_monetary_snapshots");
        builder.HasKey(x => x.FlightOfferSnapshotId);

        builder.Property(x => x.FlightOfferSnapshotId)
            .HasColumnName("flight_offer_snapshot_id")
            .HasConversion(id => id.Value, value => FlightOfferSnapshotId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Ignore(x => x.CurrencyCode);
        builder.OwnsRequiredMoney(x => x.BaseFare, "base_fare_amount", "base_fare_currency_code");
        builder.OwnsRequiredMoney(x => x.Taxes, "taxes_amount", "taxes_currency_code");
        builder.OwnsRequiredMoney(x => x.Fees, "fees_amount", "fees_currency_code");
        builder.OwnsRequiredMoney(x => x.Total, "total_amount", "currency_code");

        builder.Navigation(x => x.CategoryFares)
            .HasField("_categoryFares")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.FlightBookingId)
            .IsUnique()
            .HasDatabaseName("ux_flight_booking_monetary_snapshots_flight_booking_id");
    }
}
