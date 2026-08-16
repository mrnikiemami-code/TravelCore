using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Persistence;

internal sealed class SeoRedirectCandidateConfiguration : IEntityTypeConfiguration<SeoRedirectCandidate>
{
    public void Configure(EntityTypeBuilder<SeoRedirectCandidate> builder)
    {
        builder.ToTable("seo_redirect_candidates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SeoRedirectCandidateId.From(value));

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

        builder.Property(x => x.FromPath)
            .HasColumnName("from_path")
            .HasMaxLength(SeoRoute.PathMaxLength)
            .IsRequired();

        builder.Property(x => x.ToPath)
            .HasColumnName("to_path")
            .HasMaxLength(SeoRoute.PathMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.SeoRouteId)
            .HasDatabaseName("ix_seo_redirect_candidates_route");

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId, x.Locale })
            .HasDatabaseName("ix_seo_redirect_candidates_resource_locale");
    }
}
