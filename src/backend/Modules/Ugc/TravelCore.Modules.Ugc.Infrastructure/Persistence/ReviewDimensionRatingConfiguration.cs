using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Persistence;

internal sealed class ReviewDimensionRatingConfiguration : IEntityTypeConfiguration<ReviewDimensionRating>
{
    public void Configure(EntityTypeBuilder<ReviewDimensionRating> builder)
    {
        builder.ToTable("review_dimension_ratings");
        builder.HasKey(x => new { x.ReviewId, x.DimensionCode });

        builder.Property(x => x.ReviewId)
            .HasColumnName("review_id")
            .HasConversion(id => id.Value, value => ReviewId.From(value));

        builder.Property(x => x.DimensionCode)
            .HasColumnName("dimension_code")
            .HasMaxLength(ReviewDimensionCode.MaxLength)
            .HasConversion(code => code.Value, value => ReviewDimensionCode.Parse(value));

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasConversion(v => (short)v.Value, v => RatingValue.From(v))
            .HasColumnType("smallint")
            .IsRequired();
    }
}
