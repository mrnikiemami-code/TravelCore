using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Persistence;

internal sealed class TourDepartureAccommodationOptionConfiguration
    : IEntityTypeConfiguration<TourDepartureAccommodationOption>
{
    public void Configure(EntityTypeBuilder<TourDepartureAccommodationOption> builder)
    {
        builder.ToTable("tour_departure_accommodation_options");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TourDepartureAccommodationOptionId.From(value));

        builder.Property(x => x.TourDepartureId)
            .HasColumnName("tour_departure_id")
            .HasConversion(id => id.Value, value => TourDepartureId.From(value))
            .IsRequired();

        // Logical PlaceId only — deliberately no FK / navigation to Place.
        builder.Property(x => x.PlaceId)
            .HasColumnName("place_id")
            .IsRequired();

        builder.Property(x => x.Nights)
            .HasColumnName("nights")
            .IsRequired();

        builder.Property(x => x.BoardType)
            .HasColumnName("board_type")
            .HasConversion<short>()
            .IsRequired();

        builder.HasIndex(x => x.PlaceId)
            .HasDatabaseName("ix_tour_departure_accommodation_options_place_id");

        builder.HasIndex(x => x.TourDepartureId)
            .HasDatabaseName("ix_tour_departure_accommodation_options_departure_id");
    }
}
