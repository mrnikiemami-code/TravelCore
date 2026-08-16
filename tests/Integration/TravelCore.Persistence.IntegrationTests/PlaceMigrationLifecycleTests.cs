using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Place.Infrastructure;
using TravelCore.Modules.Place.Infrastructure.Services;
using TravelCore.Modules.ReferenceData.Contracts;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Place migration + catalog / localization / Destination / geo /
/// facilities · classification · catalog status · Place↔Media links (TC-P07-T005).
/// </summary>
[Collection(nameof(PlaceMigrationLifecycleCollection))]
public sealed class PlaceMigrationLifecycleTests
{
    private readonly PlaceMigrationLifecycleContainerFixture _postgres;

    public PlaceMigrationLifecycleTests(PlaceMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task PlaceMigrationLifecycle_Apply_And_Persist_Hotel_EndToEnd()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(5, expectedMigrations.Length);
            Assert.EndsWith("_InitialPlaceScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddPlaceCatalogTables", expectedMigrations[1], StringComparison.Ordinal);
            Assert.EndsWith("_PlaceTranslationsDestinationLinkAndGeo", expectedMigrations[2], StringComparison.Ordinal);
            Assert.EndsWith("_PlaceFacilitiesClassificationAndCatalogStatus", expectedMigrations[3], StringComparison.Ordinal);
            Assert.EndsWith("_PlaceMediaLinks", expectedMigrations[4], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await PlaceMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'place';
                """, ct));
            Assert.Equal(8, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'place'
                  AND table_name IN ('places', 'hotels', 'restaurants', 'attractions', 'place_translations', 'place_facilities', 'place_media_links', '__EFMigrationsHistory');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_name = ccu.constraint_name
                 AND tc.table_schema = ccu.table_schema
                WHERE tc.table_schema = 'place'
                  AND tc.table_name = 'place_media_links'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema = 'media';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_name = ccu.constraint_name
                 AND tc.table_schema = ccu.table_schema
                WHERE tc.table_schema = 'place'
                  AND tc.table_name = 'places'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema = 'destination';
                """, ct));
        }

        var knownDestinationId = Guid.Parse("01900000-0000-7000-8000-0000000000bb");
        var readyMediaA = Guid.Parse("01900000-0000-7000-8000-000000000501");
        var readyMediaB = Guid.Parse("01900000-0000-7000-8000-000000000502");
        var readyMediaC = Guid.Parse("01900000-0000-7000-8000-000000000503");
        var destinations = new StubDestinationExistence(knownDestinationId);
        var locales = new StubReferenceDataCatalog(("fa", "Persian"), ("en", "English"));
        var mediaReady = new StubMediaReadiness(readyMediaA, readyMediaB, readyMediaC);
        var mediaPresentation = new StubMediaPresentation();

        Guid createdId;
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new PlaceApplicationService(
                db,
                SystemClock.Instance,
                destinations,
                locales,
                mediaReady,
                mediaPresentation);
            var created = await service.CreateAsync(
                new CreatePlaceRequest(
                    "Hotel",
                    "HTL-DEMO-1",
                    "Demo Grand Hotel",
                    StarRating: 5,
                    DestinationId: knownDestinationId),
                ct);
            createdId = created.Id;

            Assert.Equal("Hotel", created.Kind);
            Assert.Equal("HTL-DEMO-1", created.Code);
            Assert.Equal("Demo Grand Hotel", created.EnglishName);
            Assert.Equal("Draft", created.CatalogStatus);
            Assert.Null(created.ClassificationCode);
            Assert.Empty(created.Facilities);
            Assert.Equal(knownDestinationId, created.DestinationId);
            Assert.NotNull(created.Hotel);
            Assert.Equal((short)5, created.Hotel.StarRating);
            Assert.Null(created.Restaurant);
            Assert.Null(created.Attraction);

            var byId = await service.GetByIdAsync(createdId, ct);
            Assert.NotNull(byId);
            Assert.Equal(createdId, byId.Id);
            Assert.Equal((short)5, byId.Hotel!.StarRating);
            Assert.Equal(knownDestinationId, byId.DestinationId);

            var fa = await service.UpsertTranslationAsync(
                createdId,
                "fa",
                new UpsertPlaceTranslationRequest("هتل دمو", "توضیح"),
                ct);
            Assert.Equal("fa", fa.LocaleCode);
            Assert.Equal("هتل دمو", fa.Name);

            await service.UpsertTranslationAsync(
                createdId,
                "en",
                new UpsertPlaceTranslationRequest("Demo Grand Hotel", "English blurb"),
                ct);

            var localizedFa = await service.GetByIdAsync(createdId, "fa", ct);
            Assert.NotNull(localizedFa);
            Assert.Equal("هتل دمو", localizedFa.LocalizedName);
            Assert.Equal("توضیح", localizedFa.LocalizedDescription);
            Assert.Equal("fa", localizedFa.Locale);
            Assert.Equal("Demo Grand Hotel", localizedFa.EnglishName);

            var localizedMissing = await service.GetByIdAsync(createdId, "ar", ct);
            Assert.NotNull(localizedMissing);
            Assert.Null(localizedMissing.LocalizedName);
            Assert.Null(localizedMissing.Locale);

            var withGeo = await service.SetGeoAsync(
                createdId,
                new SetPlaceGeoRequest(35.6892m, 51.3890m),
                ct);
            Assert.Equal(35.689200m, withGeo.Latitude);
            Assert.Equal(51.389000m, withGeo.Longitude);

            var withAddress = await service.SetAddressAsync(
                createdId,
                new SetPlaceAddressRequest(
                    "Valiasr St",
                    null,
                    "Tehran",
                    "Tehran",
                    "12345",
                    "IR"),
                ct);
            Assert.NotNull(withAddress.Address);
            Assert.Equal("Valiasr St", withAddress.Address!.Line1);
            Assert.Equal("Tehran", withAddress.Address.Locality);
            Assert.Equal("IR", withAddress.Address.CountryCode);

            var activated = await service.SetCatalogStatusAsync(
                createdId,
                new SetPlaceCatalogStatusRequest("Active"),
                ct);
            Assert.Equal("Active", activated.CatalogStatus);

            var classified = await service.SetClassificationAsync(
                createdId,
                new SetPlaceClassificationRequest("boutique-hotel"),
                ct);
            Assert.Equal("boutique-hotel", classified.ClassificationCode);

            var withFacilities = await service.SetFacilitiesAsync(
                createdId,
                new SetPlaceFacilitiesRequest(["WiFi", "parking", "wifi", "pool"]),
                ct);
            Assert.Equal(["parking", "pool", "wifi"], withFacilities.Facilities);

            var readBack = await service.GetByIdAsync(createdId, ct);
            Assert.NotNull(readBack);
            Assert.Equal("Active", readBack.CatalogStatus);
            Assert.Equal("boutique-hotel", readBack.ClassificationCode);
            Assert.Equal(["parking", "pool", "wifi"], readBack.Facilities);

            var cover = await service.SetCoverAsync(
                createdId,
                new SetPlaceCoverRequest(readyMediaA),
                ct);
            Assert.Equal("Cover", cover.Role);
            Assert.Equal(0, cover.SortOrder);
            Assert.Equal(readyMediaA, cover.MediaAssetId);

            await service.AddGalleryItemAsync(
                createdId,
                new AddPlaceGalleryItemRequest(readyMediaB, SortOrder: 10),
                ct);
            await service.AddGalleryItemAsync(
                createdId,
                new AddPlaceGalleryItemRequest(readyMediaC, SortOrder: 20),
                ct);

            var reordered = await service.ReorderGalleryAsync(
                createdId,
                new ReorderPlaceGalleryRequest([readyMediaC, readyMediaB]),
                ct);
            Assert.Equal([readyMediaC, readyMediaB], reordered.Select(x => x.MediaAssetId).ToArray());
            Assert.Equal([0, 1], reordered.Select(x => x.SortOrder).ToArray());

            var links = await service.ListMediaLinksAsync(createdId, ct);
            Assert.Contains(links, x => x.Role == "Cover" && x.MediaAssetId == readyMediaA);
            Assert.Equal(3, links.Count);

            var presentation = await service.GetMediaPresentationAsync(createdId, "fa", ct);
            Assert.NotNull(presentation);
            Assert.Equal(readyMediaA, presentation!.Cover!.MediaAssetId);
            Assert.NotNull(presentation.Cover.Presentation);
            Assert.Equal(
                MediaPresentationUrls.OriginalContent(readyMediaA),
                presentation.Cover.Presentation!.OriginalContentUrl);
            Assert.Equal(2, presentation.Gallery.Count);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SetCoverAsync(
                    createdId,
                    new SetPlaceCoverRequest(Guid.Parse("01900000-0000-7000-8000-000000000599")),
                    ct));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SetCatalogStatusAsync(
                    createdId,
                    new SetPlaceCatalogStatusRequest("Archived"),
                    ct));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SetDestinationLinkAsync(
                    createdId,
                    new SetPlaceDestinationLinkRequest(Guid.Parse("01900000-0000-7000-8000-0000000000ff")),
                    ct));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SetDestinationLinkAsync(
                    createdId,
                    new SetPlaceDestinationLinkRequest(Guid.Empty),
                    ct));

            var cleared = await service.SetDestinationLinkAsync(
                createdId,
                new SetPlaceDestinationLinkRequest(null),
                ct);
            Assert.Null(cleared.DestinationId);

            var restaurant = await service.CreateAsync(
                new CreatePlaceRequest(
                    "Restaurant",
                    "RST-DEMO-1",
                    "Demo Bistro",
                    CuisineType: "mediterranean"),
                ct);
            Assert.Equal("Restaurant", restaurant.Kind);
            Assert.Equal("mediterranean", restaurant.Restaurant!.CuisineType);
            Assert.Null(restaurant.DestinationId);

            var hotels = await service.ListAsync(kind: "Hotel", take: 10, cancellationToken: ct);
            Assert.Contains(hotels, x => x.Id == createdId);
            Assert.DoesNotContain(hotels, x => x.Id == restaurant.Id);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAsync(
                    new CreatePlaceRequest(
                        "Hotel",
                        "HTL-BAD",
                        "Bad",
                        CuisineType: "should-not-be-here"),
                    ct));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync(
                    new CreatePlaceRequest("Hotel", "HTL-DEMO-1", "Duplicate Code", StarRating: 3),
                    ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(2, await db.Places.CountAsync(ct));
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM place.hotels WHERE place_id = @id;
                """, ct, createdId));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM place.place_translations WHERE place_id = @id;
                """, ct, createdId));
            Assert.Equal(3, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM place.place_facilities WHERE place_id = @id;
                """, ct, createdId));
            Assert.Equal(3, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM place.place_media_links WHERE place_id = @id;
                """, ct, createdId));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'place'
                  AND table_name = 'place_media_links'
                  AND column_name IN ('storage_key', 'content_url', 'presigned_url');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM place.restaurants;
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM place.attractions;
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'destination'
                  AND table_name = 'places';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'place'
                  AND table_name = 'places'
                  AND column_name IN ('name_fa', 'name_en', 'rooms', 'availability', 'reservation_id', 'is_deleted', 'deleted_at', 'archived_at');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'place'
                  AND table_name = 'places'
                  AND column_name = 'destination_id';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'place'
                  AND table_name = 'places'
                  AND column_name = 'catalog_status';
                """, ct));
        }
    }

    private static async Task<int> ScalarIntAsync(
        DbConnection conn,
        string sql,
        CancellationToken cancellationToken,
        Guid? id = null)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (id is not null)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = "id";
            p.Value = id.Value;
            cmd.Parameters.Add(p);
        }

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private sealed class StubDestinationExistence(Guid knownId) : IDestinationExistenceQuery
    {
        public Task<bool> ExistsAsync(Guid destinationId, CancellationToken cancellationToken = default)
            => Task.FromResult(destinationId == knownId);
    }

    private sealed class StubMediaReadiness(params Guid[] readyIds) : IMediaAssetReadinessQuery
    {
        private readonly HashSet<Guid> _ready = readyIds.ToHashSet();

        public Task<bool> IsReadyAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
            => Task.FromResult(mediaAssetId != Guid.Empty && _ready.Contains(mediaAssetId));
    }

    private sealed class StubMediaPresentation : IMediaPresentationService
    {
        public Task<MediaAssetPresentationResponse?> GetPresentationAsync(
            Guid mediaAssetId,
            string? localeCode = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<MediaAssetPresentationResponse?>(
                new MediaAssetPresentationResponse(
                    mediaAssetId,
                    "Ready",
                    MediaPresentationUrls.OriginalContent(mediaAssetId),
                    Width: 800,
                    Height: 600,
                    FocalX: null,
                    FocalY: null,
                    ContentType: "image/jpeg",
                    Variants: [],
                    AltCaption: null));
    }

    private sealed class StubReferenceDataCatalog(params (string Code, string EnglishName)[] locales)
        : IReferenceDataCatalogQuery
    {
        private readonly Dictionary<string, LocaleCatalogItem> _locales = locales
            .ToDictionary(
                x => x.Code,
                x => new LocaleCatalogItem(x.Code, x.EnglishName),
                StringComparer.OrdinalIgnoreCase);

        public Task<IReadOnlyList<CurrencyCatalogItem>> ListCurrenciesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CurrencyCatalogItem>>([]);

        public Task<CurrencyCatalogItem?> GetCurrencyAsync(string code, CancellationToken cancellationToken = default)
            => Task.FromResult<CurrencyCatalogItem?>(null);

        public Task<IReadOnlyList<LocaleCatalogItem>> ListLocalesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<LocaleCatalogItem>>(_locales.Values.ToList());

        public Task<LocaleCatalogItem?> GetLocaleAsync(string code, CancellationToken cancellationToken = default)
            => Task.FromResult(_locales.TryGetValue(code, out var item) ? item : null);

        public Task<IReadOnlyList<CountryCatalogItem>> ListCountriesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CountryCatalogItem>>([]);

        public Task<CountryCatalogItem?> GetCountryAsync(string alpha2Code, CancellationToken cancellationToken = default)
            => Task.FromResult<CountryCatalogItem?>(null);

        public Task<IReadOnlyList<TimeZoneCatalogItem>> ListTimeZonesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TimeZoneCatalogItem>>([]);

        public Task<TimeZoneCatalogItem?> GetTimeZoneAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<TimeZoneCatalogItem?>(null);
    }
}
