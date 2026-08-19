using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Persistence;

internal sealed class SeoContentGraphNodeConfiguration : IEntityTypeConfiguration<SeoContentGraphNode>
{
    public void Configure(EntityTypeBuilder<SeoContentGraphNode> builder)
    {
        builder.ToTable("seo_content_graph_nodes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SeoContentGraphNodeId.From(value));

        builder.Property(x => x.ResourceType)
            .HasColumnName("resource_type")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.ResourceId)
            .HasColumnName("resource_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId })
            .IsUnique()
            .HasDatabaseName("ux_seo_content_graph_nodes_resource");
    }
}
