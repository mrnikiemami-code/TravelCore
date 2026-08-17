using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;

namespace TravelCore.Modules.TripPlanner.Infrastructure.Persistence;

internal sealed class TripIntentConfiguration : IEntityTypeConfiguration<TripIntent>
{
    public void Configure(EntityTypeBuilder<TripIntent> builder)
    {
        builder.ToTable("trip_intents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TripIntentId.From(value));

        builder.Property(x => x.DraftAccessToken)
            .HasColumnName("draft_access_token")
            .HasConversion(
                token => token.Value,
                value => TripIntentDraftAccessToken.FromStored(value))
            .HasMaxLength(TripIntentDraftAccessToken.StoredValueMaxLength)
            .IsRequired();

        builder.Property(x => x.ActorReference)
            .HasColumnName("actor_reference_id")
            .HasConversion(
                reference => reference.HasValue ? reference.Value.ActorId : (Guid?)null,
                value => value.HasValue ? new PlannerActorReference(value.Value) : null);

        builder.Property(x => x.PlanningRevision)
            .HasColumnName("planning_revision")
            .IsRequired();

        builder.Property(x => x.PlanningNote)
            .HasColumnName("planning_note")
            .HasMaxLength(TripIntent.PlanningNoteMaxLength);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.OwnsTravelPreferences(x => x.Preferences, "preference");

        builder.HasIndex(x => x.DraftAccessToken)
            .IsUnique()
            .HasDatabaseName("ux_trip_intents_draft_access_token");
    }
}
