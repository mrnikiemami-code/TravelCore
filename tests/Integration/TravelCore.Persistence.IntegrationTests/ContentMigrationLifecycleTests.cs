using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Infrastructure;
using TravelCore.Modules.Content.Infrastructure.Services;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Content migration + ContentItem catalog persistence smoke (TC-P08-T002).
/// </summary>
[Collection(nameof(ContentMigrationLifecycleCollection))]
public sealed class ContentMigrationLifecycleTests
{
    private readonly ContentMigrationLifecycleContainerFixture _postgres;

    public ContentMigrationLifecycleTests(ContentMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task ContentMigrationLifecycle_Apply_And_Persist_Article_EndToEnd()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(2, expectedMigrations.Length);
            Assert.EndsWith("_InitialContentScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddContentCatalogTables", expectedMigrations[1], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await ContentMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'content';
                """, ct));
            Assert.Equal(5, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'content'
                  AND table_name IN ('content_items', 'articles', 'landing_pages', 'guides', '__EFMigrationsHistory');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        Guid createdId;
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new ContentItemApplicationService(db, SystemClock.Instance);
            var created = await service.CreateAsync(
                new CreateContentItemRequest(
                    "Article",
                    "ART-DEMO-1",
                    "Demo Article"),
                ct);
            createdId = created.Id;

            Assert.Equal("Article", created.Kind);
            Assert.Equal("ART-DEMO-1", created.Code);
            Assert.Equal("Demo Article", created.EnglishName);
            Assert.NotNull(created.Article);
            Assert.Null(created.LandingPage);
            Assert.Null(created.Guide);

            var byId = await service.GetByIdAsync(createdId, ct);
            Assert.NotNull(byId);
            Assert.Equal(createdId, byId.Id);
            Assert.NotNull(byId.Article);

            var landing = await service.CreateAsync(
                new CreateContentItemRequest(
                    "LandingPage",
                    "LND-DEMO-1",
                    "Demo Landing"),
                ct);
            Assert.Equal("LandingPage", landing.Kind);
            Assert.NotNull(landing.LandingPage);

            var articles = await service.ListAsync(kind: "Article", take: 10, cancellationToken: ct);
            Assert.Contains(articles, x => x.Id == createdId);
            Assert.DoesNotContain(articles, x => x.Id == landing.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync(
                    new CreateContentItemRequest("Article", "ART-DEMO-1", "Duplicate Code"),
                    ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(2, await db.ContentItems.CountAsync(ct));
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM content.articles WHERE content_item_id = @id;
                """, ct, createdId));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM content.landing_pages;
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM content.guides;
                """, ct));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'content'
                  AND table_name = 'content_items'
                  AND column_name IN ('title_fa', 'title_en', 'slug', 'body_json', 'index_policy');
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
