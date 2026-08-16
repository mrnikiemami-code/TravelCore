using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Persistence;

internal sealed class MediaVariantConfiguration : IEntityTypeConfiguration<MediaVariant>
{
    public void Configure(EntityTypeBuilder<MediaVariant> builder)
    {
        builder.ToTable("media_variants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => MediaVariantId.From(value));

        builder.Property(x => x.MediaAssetId)
            .HasColumnName("media_asset_id")
            .HasConversion(id => id.Value, value => MediaAssetId.From(value))
            .IsRequired();

        builder.Property(x => x.Profile)
            .HasColumnName("profile")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Width)
            .HasColumnName("width");

        builder.Property(x => x.Height)
            .HasColumnName("height");

        builder.Property(x => x.ByteSize)
            .HasColumnName("byte_size");

        builder.Property(x => x.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(MediaVariant.StorageKeyMaxLength);

        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(MediaVariant.ContentTypeMaxLength);

        builder.Property(x => x.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(MediaVariant.FailureReasonMaxLength);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => new { x.MediaAssetId, x.Profile })
            .IsUnique()
            .HasDatabaseName("ux_media_variants_asset_profile");

        builder.HasIndex(x => x.StorageKey)
            .IsUnique()
            .HasFilter("storage_key IS NOT NULL")
            .HasDatabaseName("ux_media_variants_storage_key");

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(x => x.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_media_variants_media_assets");
    }
}
