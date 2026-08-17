using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Persistence;

internal sealed class TourExperienceSpecializationConfiguration
    : IEntityTypeConfiguration<TourExperienceSpecialization>
{
    public void Configure(EntityTypeBuilder<TourExperienceSpecialization> builder)
    {
        builder.ToTable("tour_experience_specializations");
        builder.HasKey(x => x.TourProductId);

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<TourProduct>()
            .WithOne()
            .HasForeignKey<TourExperienceSpecialization>(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
