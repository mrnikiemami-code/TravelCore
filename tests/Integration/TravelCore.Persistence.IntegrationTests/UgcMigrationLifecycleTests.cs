using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Ugc.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL UGC Review + dimension ratings (TC-P16-T002). Schema ugc only; no peer FK.
/// </summary>
[Collection(nameof(UgcMigrationLifecycleCollection))]
public sealed class UgcMigrationLifecycleTests
{
    private readonly UgcMigrationLifecycleContainerFixture _postgres;

    public UgcMigrationLifecycleTests(UgcMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task UgcMigrationLifecycle_Apply_Creates_Review_Tables_Without_Peer_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(7, expectedMigrations.Length);
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_InitialUgcScaffolding", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddReviewRatingBaseline", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddReviewTargetAttachment", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddTravelogueBaseline", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddUserPhotoBaseline", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddCommentBaseline", StringComparison.Ordinal));
            Assert.Contains(
                expectedMigrations,
                m => m.EndsWith("_AddUgcModerationPublicationReport", StringComparison.Ordinal));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await UgcMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'ugc';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name = 'reviews';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name = 'review_dimension_ratings';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name = 'travelogues';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name = 'user_photos';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name = 'comments';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name = 'reports';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'ugc'
                  AND table_name IN ('ratings', 'likes');
                """, ct));
            Assert.Equal(4, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'travelogues'
                  AND column_name IN ('actor_id', 'locale_code', 'title', 'body');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'travelogues'
                  AND column_name IN ('content_item_id', 'is_user_generated', 'target_id');
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'travelogues'
                  AND column_name IN ('moderation_status', 'publication_status');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'user_photos'
                  AND column_name = 'media_asset_id';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND column_name IN ('storage_key', 'mime_type', 'file_size', 'width', 'height', 'focal_point');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'comments'
                  AND column_name IN ('parent_comment_id', 'like_count');
                """, ct));
            Assert.Equal(2, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'comments'
                  AND column_name IN ('moderation_status', 'publication_status');
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'reviews'
                  AND column_name = 'overall_rating';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'reviews'
                  AND column_name = 'target_type';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND table_name = 'reviews'
                  AND column_name = 'target_id';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'ugc'
                  AND column_name IN ('hotel_rating', 'guide_rating', 'food_rating', 'service_rating');
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'ugc'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('identity', 'party', 'tour', 'place', 'destination', 'content', 'media', 'seo', 'search', 'agency_marketplace');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("ugc", db.Model.GetDefaultSchema());
        }
    }

    private static async Task<int> ScalarIntAsync(
        DbConnection conn,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }
}
