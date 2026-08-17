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

        builder.Property(x => x.Difficulty)
            .HasColumnName("difficulty")
            .HasConversion<short?>();

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

        builder.Ignore(x => x.AccommodationPlanOrdered);

        builder.HasMany(x => x.AccommodationPlan)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.AccommodationPlan)
            .HasField("_accommodationPlan")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.EligibilityRequirements)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EligibilityRequirements)
            .HasField("_eligibility")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.Equipment)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Equipment)
            .HasField("_equipment")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.LocalTransport)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.LocalTransport)
            .HasField("_localTransport")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.GuideAssignments)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.GuideAssignments)
            .HasField("_guideAssignments")
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
        builder.Ignore(x => x.MealsOrdered);

        builder.HasMany(x => x.Stops)
            .WithOne()
            .HasForeignKey(x => x.ItineraryDayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Stops)
            .HasField("_stops")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.Meals)
            .WithOne()
            .HasForeignKey(x => x.ItineraryDayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Meals)
            .HasField("_meals")
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

        // Logical DestinationId / PlaceId only — deliberately no FK / navigation (P10-R2).
        builder.Property(x => x.DestinationId)
            .HasColumnName("destination_id");

        builder.Property(x => x.PlaceId)
            .HasColumnName("place_id");

        builder.HasIndex(x => new { x.ItineraryDayId, x.SortOrder })
            .IsUnique()
            .HasDatabaseName("ux_tour_experience_itinerary_stops_day_sort");

        builder.HasIndex(x => x.DestinationId)
            .HasDatabaseName("ix_tour_experience_itinerary_stops_destination_id");

        builder.HasIndex(x => x.PlaceId)
            .HasDatabaseName("ix_tour_experience_itinerary_stops_place_id");
    }
}

internal sealed class ExperienceDayMealConfiguration : IEntityTypeConfiguration<ExperienceDayMeal>
{
    public void Configure(EntityTypeBuilder<ExperienceDayMeal> builder)
    {
        builder.ToTable("tour_experience_day_meals");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ExperienceDayMealId.From(value));

        builder.Property(x => x.ItineraryDayId)
            .HasColumnName("itinerary_day_id")
            .HasConversion(id => id.Value, value => ItineraryDayId.From(value))
            .IsRequired();

        builder.Property(x => x.MealType)
            .HasColumnName("meal_type")
            .HasConversion<short>()
            .IsRequired();

        builder.HasIndex(x => new { x.ItineraryDayId, x.MealType })
            .IsUnique()
            .HasDatabaseName("ux_tour_experience_day_meals_day_type");
    }
}

internal sealed class ExperienceAccommodationPlanEntryConfiguration
    : IEntityTypeConfiguration<ExperienceAccommodationPlanEntry>
{
    public void Configure(EntityTypeBuilder<ExperienceAccommodationPlanEntry> builder)
    {
        builder.ToTable("tour_experience_accommodation_plan");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ExperienceAccommodationPlanId.From(value));

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value))
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        // Logical PlaceId only — deliberately no FK / navigation (P10-R3).
        builder.Property(x => x.PlaceId)
            .HasColumnName("place_id");

        builder.HasIndex(x => new { x.TourProductId, x.SortOrder })
            .IsUnique()
            .HasDatabaseName("ux_tour_experience_accommodation_plan_tour_sort");

        builder.HasIndex(x => x.PlaceId)
            .HasDatabaseName("ix_tour_experience_accommodation_plan_place_id");
    }
}

internal sealed class ExperienceEligibilityRequirementConfiguration
    : IEntityTypeConfiguration<ExperienceEligibilityRequirement>
{
    public void Configure(EntityTypeBuilder<ExperienceEligibilityRequirement> builder)
    {
        builder.ToTable("tour_experience_eligibility_requirements");
        builder.HasKey(x => new { x.TourProductId, x.Code });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(TourCatalogFactCode.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasMaxLength(ExperienceEligibilityRequirement.ValueMaxLength);

        builder.Property(x => x.Detail)
            .HasColumnName("detail")
            .HasMaxLength(TourCatalogFactCode.DetailMaxLength);
    }
}

internal sealed class ExperienceEquipmentItemConfiguration : IEntityTypeConfiguration<ExperienceEquipmentItem>
{
    public void Configure(EntityTypeBuilder<ExperienceEquipmentItem> builder)
    {
        builder.ToTable("tour_experience_equipment");
        builder.HasKey(x => new { x.TourProductId, x.Code });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(TourCatalogFactCode.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Detail)
            .HasColumnName("detail")
            .HasMaxLength(TourCatalogFactCode.DetailMaxLength);
    }
}

internal sealed class ExperienceLocalTransportItemConfiguration
    : IEntityTypeConfiguration<ExperienceLocalTransportItem>
{
    public void Configure(EntityTypeBuilder<ExperienceLocalTransportItem> builder)
    {
        builder.ToTable("tour_experience_local_transport");
        builder.HasKey(x => new { x.TourProductId, x.Code });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(TourCatalogFactCode.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Detail)
            .HasColumnName("detail")
            .HasMaxLength(TourCatalogFactCode.DetailMaxLength);
    }
}

internal sealed class ExperienceGuideAssignmentConfiguration : IEntityTypeConfiguration<ExperienceGuideAssignment>
{
    public void Configure(EntityTypeBuilder<ExperienceGuideAssignment> builder)
    {
        builder.ToTable("tour_experience_guide_assignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ExperienceGuideAssignmentId.From(value));

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value))
            .IsRequired();

        // Logical GuidePartyId only — deliberately no FK / navigation (P10-R7).
        builder.Property(x => x.GuidePartyId)
            .HasColumnName("guide_party_id")
            .IsRequired();

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(TourCatalogFactCode.DetailMaxLength);

        builder.HasIndex(x => new { x.TourProductId, x.GuidePartyId })
            .IsUnique()
            .HasDatabaseName("ux_tour_experience_guide_assignments_tour_party");

        builder.HasIndex(x => x.GuidePartyId)
            .HasDatabaseName("ix_tour_experience_guide_assignments_guide_party_id");
    }
}
