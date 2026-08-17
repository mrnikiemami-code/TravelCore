using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
    }
}
