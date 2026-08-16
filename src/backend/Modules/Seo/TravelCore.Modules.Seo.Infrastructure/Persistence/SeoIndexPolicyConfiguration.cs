using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Persistence;

internal sealed class SeoIndexPolicyConfiguration : IEntityTypeConfiguration<SeoIndexPolicy>
{
    public void Configure(EntityTypeBuilder<SeoIndexPolicy> builder)
    {
        builder.ToTable("seo_index_policies");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SeoIndexPolicyId.From(value));

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

        builder.Property(x => x.IndexDirective)
            .HasColumnName("index_directive")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.FollowDirective)
            .HasColumnName("follow_directive")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId, x.Locale })
            .IsUnique()
            .HasDatabaseName("ux_seo_index_policies_resource_locale");
    }
}
