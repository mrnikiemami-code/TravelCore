using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelCancellationPolicySnapshotConfiguration : IEntityTypeConfiguration<HotelCancellationPolicySnapshot>
{
    public void Configure(EntityTypeBuilder<HotelCancellationPolicySnapshot> builder)
    {
        builder.ToTable("hotel_cancellation_policy_snapshots");
        builder.HasKey(x => x.HotelRateOfferSnapshotId);

        builder.Property(x => x.HotelRateOfferSnapshotId)
            .HasColumnName("hotel_rate_offer_snapshot_id")
            .HasConversion(id => id.Value, value => HotelRateOfferSnapshotId.From(value));

        builder.Property(x => x.PropertyTimeZoneId)
            .HasColumnName("property_time_zone_id")
            .HasMaxLength(HotelCancellationPolicySnapshot.TimeZoneIdMaxLength);

        builder.Property(x => x.PublicExplanation)
            .HasColumnName("public_explanation")
            .HasMaxLength(HotelCancellationPolicySnapshot.ExplanationMaxLength);

        builder.Navigation(x => x.Rules)
            .HasField("_rules")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
