using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Persistence;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => CommentId.From(value));

        builder.Property(x => x.ActorId)
            .HasColumnName("actor_id")
            .IsRequired();

        builder.Property(x => x.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(CommentTargetType.MaxLength)
            .HasConversion(type => type.Value, value => CommentTargetType.Parse(value))
            .IsRequired();

        builder.Property(x => x.TargetId)
            .HasColumnName("target_id")
            .IsRequired();

        builder.Ignore(x => x.Target);

        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasMaxLength(Comment.BodyMaxLength)
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
            "ix_comments_moderation_status",
            "ix_comments_publication_status");

        builder.HasIndex(x => x.ActorId)
            .HasDatabaseName("ix_comments_actor_id");

        builder.HasIndex(x => new { x.TargetType, x.TargetId })
            .HasDatabaseName("ix_comments_target_type_target_id");
    }
}
