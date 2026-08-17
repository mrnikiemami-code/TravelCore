using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;

namespace TravelCore.Modules.TripPlanner.Infrastructure.Persistence;

internal sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => LeadId.From(value));

        builder.Property(x => x.SourceTripIntentId)
            .HasColumnName("source_trip_intent_id")
            .HasConversion(id => id.Value, value => TripIntentId.From(value))
            .IsRequired();

        builder.Property(x => x.ActorReference)
            .HasColumnName("actor_reference_id")
            .HasConversion(
                reference => reference.HasValue ? reference.Value.ActorId : (Guid?)null,
                value => value.HasValue ? new PlannerActorReference(value.Value) : null);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<LeadStatus>(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.SubmittedAt)
            .HasColumnName("submitted_at")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.StatusChangedAt)
            .HasColumnName("status_changed_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<TripIntent>()
            .WithMany()
            .HasForeignKey(x => x.SourceTripIntentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(
            x => x.Snapshot,
            snapshot =>
            {
                snapshot.Property(x => x.CapturedPlanningRevision)
                    .HasColumnName("captured_planning_revision")
                    .IsRequired();

                snapshot.Property(x => x.CapturedPlanningNote)
                    .HasColumnName("captured_planning_note")
                    .HasMaxLength(TripIntent.PlanningNoteMaxLength);

                snapshot.OwnsOne(
                    s => s.Preferences,
                    preferences => TravelPreferencesMapping.ConfigureTravelPreferenceSnapshotOwned(
                        preferences,
                        "captured_preference"));

                snapshot.Navigation(s => s.Preferences).IsRequired();
            });

        builder.OwnsOne(
            x => x.Contact,
            contact =>
            {
                contact.Property(x => x.DisplayName)
                    .HasColumnName("contact_display_name")
                    .HasMaxLength(LeadContactSnapshot.DisplayNameMaxLength);

                contact.Property(x => x.Email)
                    .HasColumnName("contact_email")
                    .HasMaxLength(LeadContactSnapshot.EmailMaxLength);

                contact.Property(x => x.NormalizedEmail)
                    .HasColumnName("contact_normalized_email")
                    .HasMaxLength(LeadContactSnapshot.EmailMaxLength);

                contact.Property(x => x.Phone)
                    .HasColumnName("contact_phone")
                    .HasMaxLength(LeadContactSnapshot.PhoneMaxLength);
            });

        builder.Navigation(x => x.Snapshot).IsRequired();
        builder.Navigation(x => x.Contact).IsRequired();

        builder.HasIndex(x => x.SourceTripIntentId)
            .HasDatabaseName("ix_leads_source_trip_intent_id");
    }
}
