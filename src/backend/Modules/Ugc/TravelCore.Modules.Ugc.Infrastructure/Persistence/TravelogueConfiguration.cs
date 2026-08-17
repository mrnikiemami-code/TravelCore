using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Persistence;

internal sealed class TravelogueConfiguration : IEntityTypeConfiguration<Travelogue>
{
    public void Configure(EntityTypeBuilder<Travelogue> builder)
    {
        builder.ToTable("travelogues");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TravelogueId.From(value));

        builder.Property(x => x.ActorId)
            .HasColumnName("actor_id")
            .IsRequired();

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(Travelogue.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(Travelogue.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasMaxLength(Travelogue.BodyMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.ActorId)
            .HasDatabaseName("ix_travelogues_actor_id");

        builder.HasIndex(x => x.LocaleCode)
            .HasDatabaseName("ix_travelogues_locale_code");
    }
}
