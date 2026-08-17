using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Persistence;

internal sealed class TourDepartureConfiguration : IEntityTypeConfiguration<TourDeparture>
{
    public void Configure(EntityTypeBuilder<TourDeparture> builder)
    {
        builder.ToTable("tour_departures");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TourDepartureId.From(value));

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<TourProduct>()
            .WithMany()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TourProductId)
            .HasDatabaseName("ix_tour_departures_tour_product_id");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_tour_departures_created_at");
    }
}
