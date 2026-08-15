using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Destination.Domain;
using TravelCore.Modules.Destination.Infrastructure;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Destination migration + hierarchy smoke (TC-P04-T003).
/// </summary>
[Collection(nameof(DestinationMigrationLifecycleCollection))]
public sealed class DestinationMigrationLifecycleTests
{
    private readonly DestinationMigrationLifecycleContainerFixture _postgres;

    public DestinationMigrationLifecycleTests(DestinationMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task DestinationMigrationLifecycle_Apply_And_HierarchySmoke()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Single(expectedMigrations);
            Assert.EndsWith("_InitialDestinationPersistence", expectedMigrations[0], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await DestinationMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'destination';
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'destination'
                  AND table_name IN ('destinations', '__EFMigrationsHistory');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        var now = Instant.FromUtc(2026, 8, 15, 22, 30);
        DestinationId countryId;
        DestinationId cityId;

        await using (var db = _postgres.CreateDbContext())
        {
            var country = DestinationAggregate.Create(
                DestinationKind.Country,
                "IR",
                "Iran",
                now,
                isoCountryCode: "IR");
            var region = DestinationAggregate.Create(
                DestinationKind.Region,
                "IR-THR",
                "Tehran Province",
                now,
                parentId: country.Id,
                parent: country);
            var city = DestinationAggregate.Create(
                DestinationKind.City,
                "IR-THR-TEH",
                "Tehran",
                now,
                parentId: region.Id,
                parent: region);
            var area = DestinationAggregate.Create(
                DestinationKind.Area,
                "IR-THR-TEH-VAL",
                "Valiasr",
                now,
                parentId: city.Id,
                parent: city);

            db.Destinations.AddRange(country, region, city, area);
            await db.SaveChangesAsync(ct);
            countryId = country.Id;
            cityId = city.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(4, await db.Destinations.CountAsync(ct));
            var children = await db.Destinations.AsNoTracking()
                .Where(x => x.ParentId == cityId)
                .ToListAsync(ct);
            Assert.Single(children);
            Assert.Equal(DestinationKind.Area, children[0].Kind);

            var country = await db.Destinations.AsNoTracking()
                .SingleAsync(x => x.Id == countryId, ct);
            Assert.Equal("IR", country.IsoCountryCode);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await DestinationMigrator.MigrateAsync(db, ct);
            Assert.Equal(4, await db.Destinations.CountAsync(ct));
        }
    }

    private static async Task<int> ScalarIntAsync(DbConnection conn, string sql, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }
}
