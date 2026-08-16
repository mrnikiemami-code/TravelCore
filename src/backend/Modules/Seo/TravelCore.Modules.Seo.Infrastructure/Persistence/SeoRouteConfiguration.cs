using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Persistence;

internal sealed class SeoRouteConfiguration : IEntityTypeConfiguration<SeoRoute>
{
    public void Configure(EntityTypeBuilder<SeoRoute> builder)
    {
        builder.ToTable("seo_routes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SeoRouteId.From(value));

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

        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasMaxLength(SeoRoute.PathMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Same locale+path cannot bind two different resources.
        builder.HasIndex(x => new { x.Locale, x.Path })
            .IsUnique()
            .HasDatabaseName("ux_seo_routes_locale_path");

        // Same resource+locale cannot hold conflicting active paths (baseline: one path per binding).
        builder.HasIndex(x => new { x.ResourceType, x.ResourceId, x.Locale })
            .IsUnique()
            .HasDatabaseName("ux_seo_routes_resource_locale");

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId })
            .HasDatabaseName("ix_seo_routes_resource");
    }
}
