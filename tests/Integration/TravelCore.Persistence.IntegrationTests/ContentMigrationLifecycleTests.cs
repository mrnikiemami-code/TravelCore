using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Infrastructure;
using TravelCore.Modules.Content.Infrastructure.Services;
using TravelCore.Modules.ReferenceData.Contracts;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL Content migration + ContentItem catalog + localization smoke (TC-P08-T003).
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
            Assert.Equal(5, expectedMigrations.Length);
            Assert.EndsWith("_InitialContentScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddContentCatalogTables", expectedMigrations[1], StringComparison.Ordinal);
            Assert.EndsWith("_AddContentItemTranslations", expectedMigrations[2], StringComparison.Ordinal);
            Assert.EndsWith("_AddContentTaxonomyTables", expectedMigrations[3], StringComparison.Ordinal);
            Assert.EndsWith("_AddContentBlocksTables", expectedMigrations[4], StringComparison.Ordinal);
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
            Assert.Equal(13, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'content'
                  AND table_name IN (
                    'content_items',
                    'articles',
                    'landing_pages',
                    'guides',
                    'content_item_translations',
                    'content_categories',
                    'content_tags',
                    'content_item_categories',
                    'content_item_tags',
                    'content_blocks',
                    'content_block_gallery_items',
                    'content_block_faq_items',
                    '__EFMigrationsHistory');
                """, ct));
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        Guid createdId;
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new ContentItemApplicationService(
                db,
                SystemClock.Instance,
                new StubReferenceDataCatalogQuery());
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

            var translation = await service.UpsertTranslationAsync(
                createdId,
                "fa",
                new UpsertContentItemTranslationRequest(
                    "عنوان آزمایشی",
                    "بدنه آزمایشی",
                    "خلاصه آزمایشی"),
                ct);
            Assert.Equal("fa", translation.LocaleCode);
            Assert.Equal("عنوان آزمایشی", translation.Title);
            Assert.Equal("بدنه آزمایشی", translation.Body);
            Assert.Equal("خلاصه آزمایشی", translation.Excerpt);

            var listed = await service.ListTranslationsAsync(createdId, ct);
            Assert.Single(listed);
            Assert.Equal("fa", listed[0].LocaleCode);

            var localized = await service.GetByIdAsync(createdId, "fa", ct);
            Assert.NotNull(localized);
            Assert.Equal("عنوان آزمایشی", localized.LocalizedTitle);
            Assert.Equal("بدنه آزمایشی", localized.LocalizedBody);
            Assert.Equal("خلاصه آزمایشی", localized.LocalizedExcerpt);

            var missingLocale = await service.GetByIdAsync(createdId, "en", ct);
            Assert.NotNull(missingLocale);
            Assert.Null(missingLocale.LocalizedTitle);

            var taxonomy = new ContentTaxonomyApplicationService(db, SystemClock.Instance);
            var category = await taxonomy.CreateCategoryAsync(
                new CreateContentCategoryRequest("tips", "Travel Tips"),
                ct);
            var tag = await taxonomy.CreateTagAsync(
                new CreateContentTagRequest("visa", "Visa"),
                ct);
            Assert.Equal("tips", category.Code);
            Assert.Equal("visa", tag.Code);

            var assigned = await service.AssignCategoryAsync(createdId, category.Id, ct);
            assigned = await service.AssignTagAsync(createdId, tag.Id, ct);
            Assert.Contains(category.Id, assigned.CategoryIds!);
            Assert.Contains(tag.Id, assigned.TagIds!);

            var blocks = new ContentBlockApplicationService(db, SystemClock.Instance);
            var heading = await blocks.AddHeadingAsync(
                createdId,
                new AddContentHeadingBlockRequest("Intro", 2),
                ct);
            var paragraph = await blocks.AddParagraphAsync(
                createdId,
                new AddContentParagraphBlockRequest("Hello blocks"),
                ct);
            Assert.Equal("Heading", heading.Kind);
            Assert.Equal("Paragraph", paragraph.Kind);
            var listedBlocks = await blocks.ListAsync(createdId, ct);
            Assert.Equal(2, listedBlocks.Count);
            await blocks.ReorderAsync(
                createdId,
                new ReorderContentBlocksRequest([paragraph.Id, heading.Id]),
                ct);
            listedBlocks = await blocks.ListAsync(createdId, ct);
            Assert.Equal(paragraph.Id, listedBlocks[0].Id);
            Assert.Equal(0, listedBlocks[0].SortOrder);

            // Duplicate create last: a failed SaveChanges leaves the Added entity tracked.
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
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM content.content_item_translations WHERE content_item_id = @id;
                """, ct, createdId));
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'content'
                  AND table_name IN ('content_items', 'content_item_translations')
                  AND column_name IN (
                    'title_fa', 'title_en', 'body_fa', 'body_en',
                    'slug', 'body_json', 'index_policy');
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

    private sealed class StubReferenceDataCatalogQuery : IReferenceDataCatalogQuery
    {
        public Task<IReadOnlyList<CurrencyCatalogItem>> ListCurrenciesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CurrencyCatalogItem>>([]);

        public Task<CurrencyCatalogItem?> GetCurrencyAsync(
            string code,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<CurrencyCatalogItem?>(null);

        public Task<IReadOnlyList<LocaleCatalogItem>> ListLocalesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<LocaleCatalogItem>>([new("fa", "Persian"), new("en", "English")]);

        public Task<LocaleCatalogItem?> GetLocaleAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            var normalized = code.Trim().ToLowerInvariant();
            return Task.FromResult<LocaleCatalogItem?>(
                normalized is "fa" or "en"
                    ? new LocaleCatalogItem(normalized, normalized == "fa" ? "Persian" : "English")
                    : null);
        }

        public Task<IReadOnlyList<CountryCatalogItem>> ListCountriesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CountryCatalogItem>>([]);

        public Task<CountryCatalogItem?> GetCountryAsync(
            string alpha2Code,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<CountryCatalogItem?>(null);

        public Task<IReadOnlyList<TimeZoneCatalogItem>> ListTimeZonesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TimeZoneCatalogItem>>([]);

        public Task<TimeZoneCatalogItem?> GetTimeZoneAsync(
            string id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<TimeZoneCatalogItem?>(null);
    }
}
