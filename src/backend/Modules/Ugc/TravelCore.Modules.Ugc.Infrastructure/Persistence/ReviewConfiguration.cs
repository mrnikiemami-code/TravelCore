using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Persistence;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ReviewId.From(value));

        builder.Property(x => x.ActorId)
            .HasColumnName("actor_id")
            .IsRequired();

        builder.Property(x => x.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(ReviewTargetType.MaxLength)
            .HasConversion(type => type.Value, value => ReviewTargetType.Parse(value))
            .IsRequired();

        builder.Property(x => x.TargetId)
            .HasColumnName("target_id")
            .IsRequired();

        builder.Ignore(x => x.Target);

        builder.Property(x => x.OverallRating)
            .HasColumnName("overall_rating")
            .HasConversion(v => (short)v.Value, v => RatingValue.From(v))
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(Review.TitleMaxLength);

        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasMaxLength(Review.BodyMaxLength);

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
            "ix_reviews_moderation_status",
            "ix_reviews_publication_status");

        builder.HasMany(x => x.DimensionRatings)
            .WithOne()
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.DimensionRatings)
            .HasField("_dimensionRatings")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => x.ActorId)
            .HasDatabaseName("ix_reviews_actor_id");

        builder.HasIndex(x => new { x.TargetType, x.TargetId })
            .HasDatabaseName("ix_reviews_target_type_target_id");
    }
}
