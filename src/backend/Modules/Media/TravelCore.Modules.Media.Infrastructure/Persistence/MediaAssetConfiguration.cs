using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Persistence;

internal sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => MediaAssetId.From(value));

        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(MediaAsset.ContentTypeMaxLength)
            .IsRequired();

        builder.Property(x => x.ByteSize)
            .HasColumnName("byte_size")
            .IsRequired();

        builder.Property(x => x.Width)
            .HasColumnName("width");

        builder.Property(x => x.Height)
            .HasColumnName("height");

        builder.Property(x => x.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(MediaAsset.StorageKeyMaxLength);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_media_assets_status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_media_assets_created_at");

        builder.HasIndex(x => x.StorageKey)
            .IsUnique()
            .HasFilter("storage_key IS NOT NULL")
            .HasDatabaseName("ux_media_assets_storage_key");
    }
}
