using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Persistence;

internal sealed class UserPhotoConfiguration : IEntityTypeConfiguration<UserPhoto>
{
    public void Configure(EntityTypeBuilder<UserPhoto> builder)
    {
        builder.ToTable("user_photos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => UserPhotoId.From(value));

        builder.Property(x => x.ActorId)
            .HasColumnName("actor_id")
            .IsRequired();

        builder.Property(x => x.MediaAssetId)
            .HasColumnName("media_asset_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        UgcLifecycleMapping.Map(
            builder,
            x => x.ModerationStatus,
            x => x.PublicationStatus,
            "ix_user_photos_moderation_status",
            "ix_user_photos_publication_status");

        builder.HasIndex(x => x.ActorId)
            .HasDatabaseName("ix_user_photos_actor_id");

        builder.HasIndex(x => x.MediaAssetId)
            .IsUnique()
            .HasDatabaseName("ux_user_photos_media_asset_id");
    }
}
