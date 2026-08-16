using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Persistence;

internal sealed class SeoRedirectConfiguration : IEntityTypeConfiguration<SeoRedirect>
{
    public void Configure(EntityTypeBuilder<SeoRedirect> builder)
    {
        builder.ToTable("seo_redirects");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SeoRedirectId.From(value));

        builder.Property(x => x.SeoRouteId)
            .HasColumnName("seo_route_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? SeoRouteId.From(value.Value) : null);

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

        builder.Property(x => x.FromPath)
            .HasColumnName("from_path")
            .HasMaxLength(SeoRoute.PathMaxLength)
            .IsRequired();

        builder.Property(x => x.ToPath)
            .HasColumnName("to_path")
            .HasMaxLength(SeoRoute.PathMaxLength);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.SourceCandidateId)
            .HasColumnName("source_candidate_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? SeoRedirectCandidateId.From(value.Value) : null);

        // One live redirect/gone posture per locale+from_path.
        builder.HasIndex(x => new { x.Locale, x.FromPath })
            .IsUnique()
            .HasDatabaseName("ux_seo_redirects_locale_from_path");

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId, x.Locale })
            .HasDatabaseName("ix_seo_redirects_resource_locale");
    }
}
