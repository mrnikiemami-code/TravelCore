using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Tour.Domain;
using TravelCore.Modules.Tour.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Tour migration + TourProduct core + Experience specialization (TC-P09… · TC-P10-T001).
/// </summary>
[Collection(nameof(TourMigrationLifecycleCollection))]
public sealed class TourMigrationLifecycleTests
{
    private readonly TourMigrationLifecycleContainerFixture _postgres;

    public TourMigrationLifecycleTests(TourMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task TourMigrationLifecycle_Apply_And_Persist_TourProduct_EndToEnd()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(19, expectedMigrations.Length);
            Assert.EndsWith("_InitialTourScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourProductTables", expectedMigrations[1], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourProductTranslations", expectedMigrations[2], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourProductSemanticLinks", expectedMigrations[3], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourProductAgencyLink", expectedMigrations[4], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourProductCatalogFacts", expectedMigrations[5], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourProductMediaLinks", expectedMigrations[6], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourProductPublishingAndSlug", expectedMigrations[7], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourExperienceSpecialization", expectedMigrations[8], StringComparison.Ordinal);
            Assert.EndsWith("_AddExperienceItineraryStructure", expectedMigrations[9], StringComparison.Ordinal);
            Assert.EndsWith("_AddExperienceItineraryStopSemanticLinks", expectedMigrations[10], StringComparison.Ordinal);
            Assert.EndsWith("_AddExperienceMealsAndAccommodationPlan", expectedMigrations[11], StringComparison.Ordinal);
            Assert.EndsWith("_AddExperienceOperationalAttributes", expectedMigrations[12], StringComparison.Ordinal);
            Assert.EndsWith("_AddExperienceGuideAssignments", expectedMigrations[13], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourDepartureScaffolding", expectedMigrations[14], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourDepartureSchedule", expectedMigrations[15], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourDepartureCapacity", expectedMigrations[16], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourDepartureStatus", expectedMigrations[17], StringComparison.Ordinal);
            Assert.EndsWith("_AddTourDepartureTransportSegments", expectedMigrations[18], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await TourMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'tour';
                """, ct));
            Assert.Equal(20, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'tour'
                  AND table_name IN (
                    'tour_products',
                    'tour_product_translations',
                    'tour_product_destinations',
                    'tour_product_services',
                    'tour_product_policies',
                    'tour_product_requirements',
                    'tour_product_media_links',
                    'tour_experience_specializations',
                    'tour_experience_itineraries',
                    'tour_experience_itinerary_days',
                    'tour_experience_itinerary_stops',
                    'tour_experience_day_meals',
                    'tour_experience_accommodation_plan',
                    'tour_experience_eligibility_requirements',
                    'tour_experience_equipment',
                    'tour_experience_local_transport',
                    'tour_experience_guide_assignments',
                    'tour_departures',
                    'tour_departure_transport_segments',
                    '__EFMigrationsHistory');
                """, ct));
            // No Package specialty / Flight entity / HotelOption product tables yet.
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'tour'
                  AND table_name IN (
                    'experience_tours',
                    'package_tours',
                    'tour_package_specializations',
                    'flight_segments',
                    'tour_hotel_options');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'tour'
                  AND table_name = 'tour_experience_itinerary_stops'
                  AND column_name IN ('attraction_id');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'tour'
                  AND table_name = 'tour_experience_itinerary_stops'
                  AND column_name = 'destination_id';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'tour'
                  AND table_name = 'tour_experience_itinerary_stops'
                  AND column_name = 'place_id';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_name = ccu.constraint_name
                 AND tc.table_schema = ccu.table_schema
                WHERE tc.table_schema = 'tour'
                  AND tc.table_name = 'tour_experience_itinerary_stops'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('destination', 'place');
                """, ct));
            Assert.Equal(3, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'tour'
                  AND table_name = 'tour_departures'
                  AND column_name IN ('start_date', 'end_date', 'time_zone_id');
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'tour'
                  AND table_name = 'tour_departures'
                  AND column_name IN ('minimum_pax', 'maximum_pax');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'tour'
                  AND table_name = 'tour_departures'
                  AND column_name = 'status';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'tour'
                  AND table_name = 'tour_product_translations'
                  AND column_name = 'slug';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'tour'
                  AND table_name = 'tour_products'
                  AND column_name = 'catalog_status';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_name = ccu.constraint_name
                 AND tc.table_schema = ccu.table_schema
                WHERE tc.table_schema = 'tour'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('destination', 'party', 'media');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        var now = Instant.FromUtc(2026, 8, 17, 2, 30);
        TourProductId createdId;
        var originId = Guid.Parse("01900000-0000-7000-8000-000000000101");
        var destA = Guid.Parse("01900000-0000-7000-8000-000000000201");
        var destB = Guid.Parse("01900000-0000-7000-8000-000000000202");
        var agencyId = Guid.Parse("01900000-0000-7000-8000-000000000301");
        var coverId = Guid.Parse("01900000-0000-7000-8000-000000000501");
        var galleryId = Guid.Parse("01900000-0000-7000-8000-000000000502");

        await using (var db = _postgres.CreateDbContext())
        {
            var experience = TourProduct.CreateExperience("EXP-IT-001", "Caspian Walk", now);
            experience.UpsertTranslation("fa", "پیاده‌روی خزر", "توضیح", now);
            experience.UpsertTranslation("en", "Caspian Walk", "A short walk", now);
            experience.SetTranslationSlug("en", "caspian-walk", now);
            experience.SetCatalogStatus(TourCatalogStatus.Published, now);
            experience.SetClassificationCode("cultural-walk", now);
            experience.SetOriginLink(originId, now);
            experience.AssignDestination(destA, now);
            experience.AssignDestination(destB, now);
            experience.SetAgencyLink(agencyId, now);
            experience.ReplaceServices(
                [new TourCatalogFactInput("transfer", null), new TourCatalogFactInput("meals", "Breakfast")],
                now);
            experience.ReplacePolicies([new TourCatalogFactInput("cancellation", "24h notice")], now);
            experience.ReplaceRequirements([new TourCatalogFactInput("passport", "6 months validity")], now);
            experience.SetCover(coverId, now);
            experience.AddGalleryItem(galleryId, now);
            var experienceSpec = TourExperienceSpecialization.CreateFor(experience, now);
            var itinerary = experienceSpec.EnsureItinerary(now);
            var day1 = itinerary.AddDay(1, now);
            var stop = itinerary.AddStop(day1.Id, now);
            itinerary.AddStop(day1.Id, now);
            var stopDest = Guid.Parse("01900000-0000-7000-8000-000000000701");
            var stopPlace = Guid.Parse("01900000-0000-7000-8000-000000000702");
            itinerary.SetStopDestinationLink(stop.Id, stopDest, now);
            itinerary.SetStopPlaceLink(stop.Id, stopPlace, now);
            itinerary.AddMeal(day1.Id, ExperienceMealType.Breakfast, now);
            itinerary.AddMeal(day1.Id, ExperienceMealType.Lunch, now);
            var hotelPlace = Guid.Parse("01900000-0000-7000-8000-000000000801");
            experienceSpec.AddAccommodationPlanEntry(now, placeId: hotelPlace);
            experienceSpec.SetDifficulty(ExperienceDifficulty.Moderate, now);
            experienceSpec.ReplaceEligibilityRequirements([("minimum-age", "12", null)], now);
            experienceSpec.ReplaceEquipment([("hiking-shoes", ExperienceEquipmentKind.Required, null)], now);
            experienceSpec.ReplaceLocalTransport([("minibus", "Shared")], now);
            var package = TourProduct.CreatePackage("PKG-IT-001", "Istanbul Package", now);
            var departure = TourDeparture.Create(package, now);
            departure.SetSchedule(new LocalDate(2027, 5, 1), new LocalDate(2027, 5, 6), "Europe/Istanbul", now);
            departure.SetCapacity(4, 20, now);
            departure.SetStatus(TourDepartureStatus.Published, now);
            departure.AddTransportSegment(1, TourDepartureTransportMode.Air, "Tehran", "Istanbul", now);
            createdId = experience.Id;
            db.TourProducts.AddRange(experience, package);
            db.ExperienceSpecializations.Add(experienceSpec);
            db.TourDepartures.Add(departure);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.TourProducts.SingleAsync(x => x.Id == createdId, ct);
            Assert.Equal(TourKind.Experience, loaded.Kind);
            Assert.Equal("EXP-IT-001", loaded.Code);
            Assert.Equal("Caspian Walk", loaded.EnglishName);
            Assert.Equal(TourCatalogStatus.Published, loaded.CatalogStatus);
            Assert.Equal("caspian-walk", loaded.FindTranslation("en")!.Slug);
            Assert.Equal("cultural-walk", loaded.ClassificationCode);
            Assert.Equal(originId, loaded.OriginDestinationId);
            Assert.Equal(agencyId, loaded.AgencyId);
            Assert.Equal(2, loaded.Services.Count);
            Assert.Contains(loaded.Services, x => x.Code == "transfer");
            Assert.Equal("Breakfast", loaded.Services.Single(x => x.Code == "meals").Detail);
            Assert.Single(loaded.Policies);
            Assert.Equal("cancellation", loaded.Policies.Single().Code);
            Assert.Single(loaded.Requirements);
            Assert.Equal(coverId, loaded.Cover!.MediaAssetId);
            Assert.Single(loaded.GalleryOrdered);
            Assert.Equal(galleryId, loaded.GalleryOrdered[0].MediaAssetId);
            Assert.Equal(2, loaded.Destinations.Count);
            Assert.Contains(loaded.Destinations, x => x.DestinationId == destA);
            Assert.Contains(loaded.Destinations, x => x.DestinationId == destB);
            Assert.Equal(2, loaded.Translations.Count);
            Assert.Equal("پیاده‌روی خزر", loaded.FindTranslation("fa")!.Title);
            Assert.Equal("Caspian Walk", loaded.FindTranslation("en")!.Title);

            var loadedSpec = await db.ExperienceSpecializations.SingleAsync(x => x.TourProductId == createdId, ct);
            Assert.Equal(createdId, loadedSpec.TourProductId);
            Assert.Equal(now, loadedSpec.CreatedAt);
            Assert.NotNull(loadedSpec.Itinerary);
            Assert.Single(loadedSpec.Itinerary!.Days);
            Assert.Equal(1, loadedSpec.Itinerary.DaysOrdered[0].DayNumber);
            Assert.Equal(2, loadedSpec.Itinerary.DaysOrdered[0].Stops.Count);
            var linkedStop = loadedSpec.Itinerary.DaysOrdered[0].StopsOrdered[0];
            Assert.Equal(Guid.Parse("01900000-0000-7000-8000-000000000701"), linkedStop.DestinationId);
            Assert.Equal(Guid.Parse("01900000-0000-7000-8000-000000000702"), linkedStop.PlaceId);
            Assert.Null(loadedSpec.Itinerary.DaysOrdered[0].StopsOrdered[1].DestinationId);
            Assert.Equal(2, loadedSpec.Itinerary.DaysOrdered[0].Meals.Count);
            Assert.Contains(loadedSpec.Itinerary.DaysOrdered[0].Meals, x => x.MealType == ExperienceMealType.Breakfast);
            Assert.Single(loadedSpec.AccommodationPlan);
            Assert.Equal(Guid.Parse("01900000-0000-7000-8000-000000000801"), loadedSpec.AccommodationPlanOrdered[0].PlaceId);
            Assert.Equal(ExperienceDifficulty.Moderate, loadedSpec.Difficulty);
            Assert.Single(loadedSpec.EligibilityRequirements);
            Assert.Single(loadedSpec.Equipment);
            Assert.Single(loadedSpec.LocalTransport);

            var package = await db.TourProducts.SingleAsync(x => x.Code == "PKG-IT-001", ct);
            Assert.Equal(TourKind.Package, package.Kind);
            Assert.Null(package.ClassificationCode);
            Assert.Null(package.OriginDestinationId);
            Assert.Null(package.AgencyId);
            Assert.Empty(package.Destinations);
            Assert.False(await db.ExperienceSpecializations.AnyAsync(x => x.TourProductId == package.Id, ct));

            var loadedDeparture = await db.TourDepartures.SingleAsync(x => x.TourProductId == package.Id, ct);
            Assert.Equal(package.Id, loadedDeparture.TourProductId);
            Assert.Equal(now, loadedDeparture.CreatedAt);
            Assert.NotEqual(package.Id.Value, loadedDeparture.Id.Value);
            Assert.NotNull(loadedDeparture.Schedule);
            Assert.Equal(new LocalDate(2027, 5, 1), loadedDeparture.Schedule!.StartDate);
            Assert.Equal(new LocalDate(2027, 5, 6), loadedDeparture.Schedule.EndDate);
            Assert.Equal("Europe/Istanbul", loadedDeparture.Schedule.TimeZoneId);
            Assert.NotNull(loadedDeparture.Capacity);
            Assert.Equal(4, loadedDeparture.Capacity!.MinimumPax);
            Assert.Equal(20, loadedDeparture.Capacity.MaximumPax);
            Assert.Equal(TourDepartureStatus.Published, loadedDeparture.Status);
            Assert.Single(loadedDeparture.TransportSegments);
            Assert.Equal(TourDepartureTransportMode.Air, loadedDeparture.TransportSegmentsOrdered[0].TransportMode);
            Assert.Equal("Tehran", loadedDeparture.TransportSegmentsOrdered[0].Origin);
            Assert.Equal("Istanbul", loadedDeparture.TransportSegmentsOrdered[0].Destination);
        }
    }

    private static async Task<int> ScalarIntAsync(DbConnection conn, string sql, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }
}
