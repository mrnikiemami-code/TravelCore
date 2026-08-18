using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelChargeComponentSnapshotConfiguration : IEntityTypeConfiguration<HotelChargeComponentSnapshot>
{
    public void Configure(EntityTypeBuilder<HotelChargeComponentSnapshot> builder)
    {
        builder.ToTable("hotel_charge_component_snapshots");
        builder.HasKey(x => new { x.HotelRateOfferSnapshotId, x.Ordinal });

        builder.Property(x => x.HotelRateOfferSnapshotId)
            .HasColumnName("hotel_rate_offer_snapshot_id")
            .HasConversion(id => id.Value, value => HotelRateOfferSnapshotId.From(value));

        builder.Property(x => x.Ordinal)
            .HasColumnName("ordinal")
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(HotelChargeComponentSnapshot.CodeMaxLength)
            .IsRequired();

        builder.OwnsRequiredMoney(x => x.Amount, "amount", "currency_code");

        builder.HasOne<HotelBookingMonetarySnapshot>()
            .WithMany(x => x.Charges)
            .HasForeignKey(x => x.HotelRateOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
