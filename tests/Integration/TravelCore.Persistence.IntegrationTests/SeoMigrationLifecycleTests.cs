using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;
using TravelCore.Modules.Seo.Infrastructure;
using TravelCore.Modules.Seo.Infrastructure.Services;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL SEO migration + SeoRoute create/get/list smoke (TC-P05-T002).
/// </summary>
[Collection(nameof(SeoMigrationLifecycleCollection))]
public sealed class SeoMigrationLifecycleTests
{
    private readonly SeoMigrationLifecycleContainerFixture _postgres;

    public SeoMigrationLifecycleTests(SeoMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task SeoMigrationLifecycle_Apply_CreateGetList_And_SchemaSeo()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, expectedMigrations.Length);
            Assert.EndsWith("_InitialSeoScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddSeoRoutes", expectedMigrations[1], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await SeoMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'seo';
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'seo'
                  AND table_name IN ('seo_routes', '__EFMigrationsHistory');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        var destinationId = Guid.Parse("0198a000-0000-7000-8000-0000000000aa");
        var otherDestinationId = Guid.Parse("0198a000-0000-7000-8000-0000000000bb");
        Guid createdId;

        await using (var db = _postgres.CreateDbContext())
        {
            var service = new SeoRouteApplicationService(db, SystemClock.Instance);
            var created = await service.CreateAsync(
                new CreateSeoRouteRequest(
                    "Destination",
                    destinationId,
                    "en",
                    "destinations/istanbul"),
                ct);
            createdId = created.Id;

            Assert.Equal("Destination", created.ResourceType);
            Assert.Equal(destinationId, created.ResourceId);
            Assert.Equal("en", created.Locale);
            Assert.Equal("destinations/istanbul", created.Path);

            var byId = await service.GetByIdAsync(createdId, ct);
            Assert.NotNull(byId);
            Assert.Equal(createdId, byId.Id);

            var listed = await service.ListByResourceAsync("Destination", destinationId, ct);
            Assert.Single(listed);
            Assert.Equal(createdId, listed[0].Id);

            await Assert.ThrowsAsync<SeoRouteConflictException>(() =>
                service.CreateAsync(
                    new CreateSeoRouteRequest(
                        "Destination",
                        otherDestinationId,
                        "en",
                        "destinations/istanbul"),
                    ct));

            await Assert.ThrowsAsync<SeoRouteConflictException>(() =>
                service.CreateAsync(
                    new CreateSeoRouteRequest(
                        "Destination",
                        destinationId,
                        "en",
                        "destinations/istanbul-city"),
                    ct));

            var fa = await service.CreateAsync(
                new CreateSeoRouteRequest(
                    "Destination",
                    destinationId,
                    "fa",
                    "destinations/استانبول"),
                ct);
            Assert.Equal("fa", fa.Locale);
            Assert.Equal("destinations/استانبول", fa.Path);

            listed = await service.ListByResourceAsync("Destination", destinationId, ct);
            Assert.Equal(2, listed.Count);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(2, await db.SeoRoutes.CountAsync(ct));
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'seo'
                  AND table_name = 'seo_routes';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'destination'
                  AND table_name = 'seo_routes';
                """, ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await SeoMigrator.MigrateAsync(db, ct);
            Assert.Equal(2, await db.SeoRoutes.CountAsync(ct));
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
