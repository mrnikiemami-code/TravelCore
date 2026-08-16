using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Persistence;

internal sealed class MediaAssetTranslationConfiguration : IEntityTypeConfiguration<MediaAssetTranslation>
{
    public void Configure(EntityTypeBuilder<MediaAssetTranslation> builder)
    {
        builder.ToTable("media_asset_translations");
        builder.HasKey(x => new { x.MediaAssetId, x.LocaleCode });

        builder.Property(x => x.MediaAssetId)
            .HasColumnName("media_asset_id")
            .HasConversion(id => id.Value, value => MediaAssetId.From(value))
            .IsRequired();

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(MediaAssetTranslation.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(MediaAssetTranslation.AltTextMaxLength)
            .IsRequired();

        builder.Property(x => x.Caption)
            .HasColumnName("caption")
            .HasMaxLength(MediaAssetTranslation.CaptionMaxLength);

        builder.Property(x => x.PublicationStatus)
            .HasColumnName("publication_status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.LocaleCode)
            .HasDatabaseName("ix_media_asset_translations_locale_code");

        builder.HasIndex(x => new { x.MediaAssetId, x.PublicationStatus })
            .HasDatabaseName("ix_media_asset_translations_asset_publication");

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(x => x.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_media_asset_translations_media_assets");
    }
}
