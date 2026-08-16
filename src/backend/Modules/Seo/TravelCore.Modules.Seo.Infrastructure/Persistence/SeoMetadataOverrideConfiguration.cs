using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Persistence;

internal sealed class SeoMetadataOverrideConfiguration : IEntityTypeConfiguration<SeoMetadataOverride>
{
    public void Configure(EntityTypeBuilder<SeoMetadataOverride> builder)
    {
        builder.ToTable("seo_metadata_overrides");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SeoMetadataOverrideId.From(value));

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

        builder.Property(x => x.TitleOverride)
            .HasColumnName("title_override")
            .HasMaxLength(SeoMetadataOverride.TitleMaxLength);

        builder.Property(x => x.DescriptionOverride)
            .HasColumnName("description_override")
            .HasMaxLength(SeoMetadataOverride.DescriptionMaxLength);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId, x.Locale })
            .IsUnique()
            .HasDatabaseName("ux_seo_metadata_overrides_resource_locale");
    }
}
