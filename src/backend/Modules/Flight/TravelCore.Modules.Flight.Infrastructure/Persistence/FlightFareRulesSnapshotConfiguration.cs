using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightFareRulesSnapshotConfiguration : IEntityTypeConfiguration<FlightFareRulesSnapshot>
{
    public void Configure(EntityTypeBuilder<FlightFareRulesSnapshot> builder)
    {
        builder.ToTable("flight_fare_rule_snapshots");
        builder.HasKey(x => x.FlightOfferSnapshotId);

        builder.Property(x => x.FlightOfferSnapshotId)
            .HasColumnName("flight_offer_snapshot_id")
            .HasConversion(id => id.Value, value => FlightOfferSnapshotId.From(value));

        builder.Property(x => x.Refundable).HasColumnName("refundable").IsRequired();
        builder.Property(x => x.Changeable).HasColumnName("changeable").IsRequired();
        builder.Property(x => x.TicketingDeadline).HasColumnName("ticketing_deadline");
        builder.Property(x => x.PartialRefundRequired).HasColumnName("partial_refund_required").IsRequired();

        builder.OwnsOptionalMoney(x => x.CancelPenalty, "cancel_penalty_amount", "cancel_penalty_currency_code");
        builder.OwnsOptionalMoney(x => x.ChangePenalty, "change_penalty_amount", "change_penalty_currency_code");

        builder.Navigation(x => x.Baggage)
            .HasField("_baggage")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
