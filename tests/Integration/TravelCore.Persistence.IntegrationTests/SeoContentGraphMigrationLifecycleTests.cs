using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Seo.Domain;
using TravelCore.Modules.Seo.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL SEO content graph foundation (TC-P26-T004).
/// </summary>
[Collection(nameof(SeoMigrationLifecycleCollection))]
public sealed class SeoContentGraphMigrationLifecycleTests
{
    private readonly SeoMigrationLifecycleContainerFixture _postgres;

    public SeoContentGraphMigrationLifecycleTests(SeoMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task SeoContentGraphMigrationLifecycle_Apply_And_Register_Node()
    {
        var ct = TestContext.Current.CancellationToken;
        var resourceId = Guid.Parse("0198a000-0000-7000-8000-0000000000cc");

        await using (var db = _postgres.CreateDbContext())
        {
            await SeoMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'seo'
                  AND table_name = 'seo_content_graph_nodes';
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.constraint_type = 'FOREIGN KEY'
                  AND tc.table_schema = 'seo'
                  AND tc.table_name = 'seo_content_graph_nodes'
                  AND ccu.table_schema <> 'seo';
                """, ct));

            var node = SeoContentGraphNode.Register(
                SeoResourceType.Destination,
                resourceId,
                Instant.FromUtc(2026, 8, 19, 18, 30));
            db.SeoContentGraphNodes.Add(node);
            await db.SaveChangesAsync(ct);

            Assert.Equal(1, await db.SeoContentGraphNodes.CountAsync(ct));
            await Assert.ThrowsAsync<DbUpdateException>(() =>
            {
                db.SeoContentGraphNodes.Add(SeoContentGraphNode.Register(
                    SeoResourceType.Destination,
                    resourceId,
                    Instant.FromUtc(2026, 8, 19, 18, 31)));
                return db.SaveChangesAsync(ct);
            });
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
