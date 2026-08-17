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

        builder.HasOne(x => x.Itinerary)
            .WithOne()
            .HasForeignKey<ExperienceItinerary>(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Itinerary)
            .HasField("_itinerary")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}

internal sealed class ExperienceItineraryConfiguration : IEntityTypeConfiguration<ExperienceItinerary>
{
    public void Configure(EntityTypeBuilder<ExperienceItinerary> builder)
    {
        builder.ToTable("tour_experience_itineraries");
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

        builder.Ignore(x => x.DaysOrdered);

        builder.HasMany(x => x.Days)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Days)
            .HasField("_days")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}

internal sealed class ExperienceItineraryDayConfiguration : IEntityTypeConfiguration<ExperienceItineraryDay>
{
    public void Configure(EntityTypeBuilder<ExperienceItineraryDay> builder)
    {
        builder.ToTable("tour_experience_itinerary_days");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ItineraryDayId.From(value));

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value))
            .IsRequired();

        builder.Property(x => x.DayNumber)
            .HasColumnName("day_number")
            .IsRequired();

        builder.HasIndex(x => new { x.TourProductId, x.DayNumber })
            .IsUnique()
            .HasDatabaseName("ux_tour_experience_itinerary_days_tour_day");

        builder.Ignore(x => x.StopsOrdered);

        builder.HasMany(x => x.Stops)
            .WithOne()
            .HasForeignKey(x => x.ItineraryDayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Stops)
            .HasField("_stops")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}

internal sealed class ExperienceItineraryStopConfiguration : IEntityTypeConfiguration<ExperienceItineraryStop>
{
    public void Configure(EntityTypeBuilder<ExperienceItineraryStop> builder)
    {
        builder.ToTable("tour_experience_itinerary_stops");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ItineraryStopId.From(value));

        builder.Property(x => x.ItineraryDayId)
            .HasColumnName("itinerary_day_id")
            .HasConversion(id => id.Value, value => ItineraryDayId.From(value))
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(x => new { x.ItineraryDayId, x.SortOrder })
            .IsUnique()
            .HasDatabaseName("ux_tour_experience_itinerary_stops_day_sort");

        // Deliberately no DestinationId / PlaceId columns in T002 (P10-R2 deferred).
    }
}
