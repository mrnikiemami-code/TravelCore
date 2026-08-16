using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Persistence;

internal sealed class SeoPathHistoryEntryConfiguration : IEntityTypeConfiguration<SeoPathHistoryEntry>
{
    public void Configure(EntityTypeBuilder<SeoPathHistoryEntry> builder)
    {
        builder.ToTable("seo_path_history");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SeoPathHistoryId.From(value));

        builder.Property(x => x.SeoRouteId)
            .HasColumnName("seo_route_id")
            .HasConversion(id => id.Value, value => SeoRouteId.From(value))
            .IsRequired();

        builder.Property(x => x.ResourceType)
            .HasColumnName("resource_type")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.ResourceId)
            .HasColumnName("resource_id")
            .IsRequired();

        builder.Property(x => x.Locale)
            .HasColumnName("locale")
            .HasMaxLength(SeoRoute.LocaleMaxLength)
            .IsRequired();

        // Historical public path string only — not Destination.Translation.Slug SoR.
        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasMaxLength(SeoRoute.PathMaxLength)
            .IsRequired();

        builder.Property(x => x.SucceededByPath)
            .HasColumnName("succeeded_by_path")
            .HasMaxLength(SeoRoute.PathMaxLength)
            .IsRequired();

        builder.Property(x => x.RecordedAt)
            .HasColumnName("recorded_at")
            .IsRequired();

        builder.HasIndex(x => x.SeoRouteId)
            .HasDatabaseName("ix_seo_path_history_route");

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId, x.Locale })
            .HasDatabaseName("ix_seo_path_history_resource_locale");
    }
}
