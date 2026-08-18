using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingMonetarySnapshotConfiguration : IEntityTypeConfiguration<HotelBookingMonetarySnapshot>
{
    public void Configure(EntityTypeBuilder<HotelBookingMonetarySnapshot> builder)
    {
        builder.ToTable("hotel_booking_monetary_snapshots");
        builder.HasKey(x => x.HotelRateOfferSnapshotId);

        builder.Property(x => x.HotelRateOfferSnapshotId)
            .HasColumnName("hotel_rate_offer_snapshot_id")
            .HasConversion(id => id.Value, value => HotelRateOfferSnapshotId.From(value));

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Ignore(x => x.CurrencyCode);
        builder.OwnsRequiredMoney(x => x.Total, "total_amount", "currency_code");
        builder.OwnsOptionalMoney(x => x.PayableNow, "payable_now_amount", "payable_now_currency_code");
        builder.OwnsOptionalMoney(x => x.PayableAtProperty, "payable_at_property_amount", "payable_at_property_currency_code");

        builder.Navigation(x => x.Charges)
            .HasField("_charges")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.HotelBookingId)
            .IsUnique()
            .HasDatabaseName("ux_hotel_booking_monetary_snapshots_hotel_booking_id");
    }
}
