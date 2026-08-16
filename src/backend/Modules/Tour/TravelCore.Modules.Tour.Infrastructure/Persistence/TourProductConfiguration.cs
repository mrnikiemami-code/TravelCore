using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Persistence;

internal sealed class TourProductConfiguration : IEntityTypeConfiguration<TourProduct>
{
    public void Configure(EntityTypeBuilder<TourProduct> builder)
    {
        builder.ToTable("tour_products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(TourProduct.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasColumnName("english_name")
            .HasMaxLength(TourProduct.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_tour_products_code");

        builder.HasIndex(x => x.Kind)
            .HasDatabaseName("ix_tour_products_kind");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_tour_products_created_at");
    }
}
