using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelCancellationPenaltyRuleConfiguration : IEntityTypeConfiguration<HotelCancellationPenaltyRule>
{
    public void Configure(EntityTypeBuilder<HotelCancellationPenaltyRule> builder)
    {
        builder.ToTable("hotel_cancellation_penalty_rules");
        builder.HasKey(x => new { x.HotelRateOfferSnapshotId, x.Ordinal });

        builder.Property(x => x.HotelRateOfferSnapshotId)
            .HasColumnName("hotel_rate_offer_snapshot_id")
            .HasConversion(id => id.Value, value => HotelRateOfferSnapshotId.From(value));

        builder.Property(x => x.Ordinal)
            .HasColumnName("ordinal")
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .HasColumnName("effective_from")
            .IsRequired();

        builder.Property(x => x.EffectiveUntil)
            .HasColumnName("effective_until");

        builder.OwnsRequiredMoney(x => x.Penalty, "penalty_amount", "currency_code");

        builder.HasOne<HotelCancellationPolicySnapshot>()
            .WithMany(x => x.Rules)
            .HasForeignKey(x => x.HotelRateOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
