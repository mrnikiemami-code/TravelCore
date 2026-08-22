using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Persistence;

internal sealed class AgencyOfferGovernanceEventConfiguration : IEntityTypeConfiguration<AgencyOfferGovernanceEvent>
{
    public void Configure(EntityTypeBuilder<AgencyOfferGovernanceEvent> builder)
    {
        builder.ToTable("agency_offer_governance_events");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.OfferId)
            .HasColumnName("offer_id")
            .HasConversion(id => id.Value, value => AgencyOfferId.From(value))
            .IsRequired();

        builder.Property(x => x.AgencyProfileId)
            .HasColumnName("agency_profile_id")
            .HasConversion(id => id.Value, value => AgencyProfileId.From(value))
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.ActorKind)
            .HasColumnName("actor_kind")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ActorAccountId)
            .HasColumnName("actor_account_id");

        builder.Property(x => x.FromPublicationStatus)
            .HasColumnName("from_publication_status")
            .HasMaxLength(32);

        builder.Property(x => x.ToPublicationStatus)
            .HasColumnName("to_publication_status")
            .HasMaxLength(32);

        builder.Property(x => x.PolicyCode)
            .HasColumnName("policy_code")
            .HasMaxLength(64);

        builder.Property(x => x.PolicyName)
            .HasColumnName("policy_name")
            .HasMaxLength(128);

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(512);

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.HasIndex(x => new { x.OfferId, x.OccurredAt })
            .HasDatabaseName("ix_agency_offer_governance_events_offer_occurred");

        builder.HasIndex(x => x.AgencyProfileId)
            .HasDatabaseName("ix_agency_offer_governance_events_agency_profile");
    }
}
