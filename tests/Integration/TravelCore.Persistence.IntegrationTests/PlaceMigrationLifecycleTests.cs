using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Place.Infrastructure;
using TravelCore.Modules.Place.Infrastructure.Services;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Place migration + Place catalog persistence smoke (TC-P07-T002).
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
            Assert.Equal(2, expectedMigrations.Length);
            Assert.EndsWith("_InitialPlaceScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddPlaceCatalogTables", expectedMigrations[1], StringComparison.Ordinal);
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
            Assert.Equal(5, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'place'
                  AND table_name IN ('places', 'hotels', 'restaurants', 'attractions', '__EFMigrationsHistory');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        Guid createdId;
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new PlaceApplicationService(db, SystemClock.Instance);
            var created = await service.CreateAsync(
                new CreatePlaceRequest(
                    "Hotel",
                    "HTL-DEMO-1",
                    "Demo Grand Hotel",
                    StarRating: 5),
                ct);
            createdId = created.Id;

            Assert.Equal("Hotel", created.Kind);
            Assert.Equal("HTL-DEMO-1", created.Code);
            Assert.Equal("Demo Grand Hotel", created.EnglishName);
            Assert.NotNull(created.Hotel);
            Assert.Equal((short)5, created.Hotel.StarRating);
            Assert.Null(created.Restaurant);
            Assert.Null(created.Attraction);

            var byId = await service.GetByIdAsync(createdId, ct);
            Assert.NotNull(byId);
            Assert.Equal(createdId, byId.Id);
            Assert.Equal((short)5, byId.Hotel!.StarRating);

            var restaurant = await service.CreateAsync(
                new CreatePlaceRequest(
                    "Restaurant",
                    "RST-DEMO-1",
                    "Demo Bistro",
                    CuisineType: "mediterranean"),
                ct);
            Assert.Equal("Restaurant", restaurant.Kind);
            Assert.Equal("mediterranean", restaurant.Restaurant!.CuisineType);

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
                  AND column_name IN ('name_fa', 'name_en', 'rooms', 'availability', 'reservation_id');
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
}
