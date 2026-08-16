using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Media.Infrastructure.Services;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Media migration + MediaAsset metadata smoke (TC-P06-T002).
/// </summary>
[Collection(nameof(MediaMigrationLifecycleCollection))]
public sealed class MediaMigrationLifecycleTests
{
    private readonly MediaMigrationLifecycleContainerFixture _postgres;

    public MediaMigrationLifecycleTests(MediaMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task MediaMigrationLifecycle_Apply_And_Persist_MediaAsset_Metadata()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, expectedMigrations.Length);
            Assert.EndsWith("_InitialMediaScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddMediaAssets", expectedMigrations[1], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await MediaMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'media';
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'media'
                  AND table_name IN ('media_assets', '__EFMigrationsHistory');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        Guid createdId;
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new MediaAssetApplicationService(db, SystemClock.Instance);
            var created = await service.CreateAsync(
                new CreateMediaAssetRequest(
                    "image/webp",
                    4096,
                    Width: 1200,
                    Height: 800,
                    StorageKey: "originals/demo/hero.webp",
                    Status: "Ready"),
                ct);
            createdId = created.Id;

            Assert.Equal("image/webp", created.ContentType);
            Assert.Equal(4096, created.ByteSize);
            Assert.Equal(1200, created.Width);
            Assert.Equal(800, created.Height);
            Assert.Equal("originals/demo/hero.webp", created.StorageKey);
            Assert.Equal("Ready", created.Status);

            var byId = await service.GetByIdAsync(createdId, ct);
            Assert.NotNull(byId);
            Assert.Equal(createdId, byId.Id);

            var pending = await service.CreateAsync(
                new CreateMediaAssetRequest("image/png", 12),
                ct);
            Assert.Equal("PendingStorage", pending.Status);
            Assert.Null(pending.StorageKey);

            var listed = await service.ListAsync(status: "Ready", take: 10, cancellationToken: ct);
            Assert.Contains(listed, x => x.Id == createdId);
            Assert.DoesNotContain(listed, x => x.Id == pending.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync(
                    new CreateMediaAssetRequest(
                        "image/jpeg",
                        1,
                        StorageKey: "originals/demo/hero.webp",
                        Status: "Ready"),
                    ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(2, await db.MediaAssets.CountAsync(ct));
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'destination'
                  AND table_name = 'media_assets';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'seo'
                  AND table_name = 'media_assets';
                """, ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await MediaMigrator.MigrateAsync(db, ct);
            Assert.Equal(2, await db.MediaAssets.CountAsync(ct));
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
