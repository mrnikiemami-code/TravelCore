using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Media.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

public sealed class MediaAssetTranslationServiceTests
{
    [Fact]
    public async Task Upsert_Fa_And_En_PersistsDistinctLocaleRows()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var service = new MediaAssetTranslationApplicationService(db, SystemClock.Instance);
        var asset = await SeedAssetAsync(db, ct);

        var fa = await service.UpsertAsync(
            asset.Id.Value,
            "fa",
            new UpsertMediaAssetTranslationRequest("نمای استانبول", "کپشن فارسی", "Published"),
            ct);
        var en = await service.UpsertAsync(
            asset.Id.Value,
            "en",
            new UpsertMediaAssetTranslationRequest("Istanbul skyline", "English caption", "Draft"),
            ct);

        Assert.Equal("fa", fa.LocaleCode);
        Assert.Equal("نمای استانبول", fa.AltText);
        Assert.Equal("Published", fa.PublicationStatus);
        Assert.Equal("en", en.LocaleCode);
        Assert.Equal("Draft", en.PublicationStatus);

        var listed = await service.ListAsync(asset.Id.Value, ct);
        Assert.Equal(2, listed.Count);
        Assert.Contains(listed, x => x.LocaleCode == "fa");
        Assert.Contains(listed, x => x.LocaleCode == "en");
    }

    [Fact]
    public async Task GetPublishedForPresentation_ReturnsExactLocaleOnly_NoSilentInvent()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var service = new MediaAssetTranslationApplicationService(db, SystemClock.Instance);
        var asset = await SeedAssetAsync(db, ct);

        await service.UpsertAsync(
            asset.Id.Value,
            "fa",
            new UpsertMediaAssetTranslationRequest("متن فارسی", PublicationStatus: "Published"),
            ct);
        await service.UpsertAsync(
            asset.Id.Value,
            "en",
            new UpsertMediaAssetTranslationRequest("English draft alt", PublicationStatus: "Draft"),
            ct);

        var faPublished = await service.GetPublishedForPresentationAsync(asset.Id.Value, "fa", ct);
        Assert.NotNull(faPublished);
        Assert.Equal("fa", faPublished.LocaleCode);
        Assert.Equal("متن فارسی", faPublished.AltText);

        // EN Draft must not invent FA content under en locale (ADR 0008).
        var enDraft = await service.GetPublishedForPresentationAsync(asset.Id.Value, "en", ct);
        Assert.Null(enDraft);

        // Missing locale must not invent FA.
        var arMissing = await service.GetPublishedForPresentationAsync(asset.Id.Value, "ar", ct);
        Assert.Null(arMissing);
    }

    [Fact]
    public async Task Create_DefaultsToDraft_ExistenceIsNotPublication()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var service = new MediaAssetTranslationApplicationService(db, SystemClock.Instance);
        var asset = await SeedAssetAsync(db, ct);

        var created = await service.UpsertAsync(
            asset.Id.Value,
            "en",
            new UpsertMediaAssetTranslationRequest("Harbor view"),
            ct);

        Assert.Equal("Draft", created.PublicationStatus);
        Assert.Null(await service.GetPublishedForPresentationAsync(asset.Id.Value, "en", ct));

        await service.UpsertAsync(
            asset.Id.Value,
            "en",
            new UpsertMediaAssetTranslationRequest("Harbor view", PublicationStatus: "Published"),
            ct);

        var published = await service.GetPublishedForPresentationAsync(asset.Id.Value, "en", ct);
        Assert.NotNull(published);
        Assert.Equal("Harbor view", published.AltText);
    }

    [Fact]
    public async Task Upsert_MissingAsset_Throws()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var service = new MediaAssetTranslationApplicationService(db, SystemClock.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpsertAsync(
                Guid.Parse("44444444-4444-7444-8444-444444444444"),
                "fa",
                new UpsertMediaAssetTranslationRequest("alt"),
                ct));
    }

    [Fact]
    public void NormalizeLocale_And_RejectEmptyAlt()
    {
        Assert.Equal("fa", MediaAssetTranslation.NormalizeLocaleCode("FA"));
        Assert.Equal("en-US", MediaAssetTranslation.NormalizeLocaleCode("en-us"));
        Assert.Throws<ArgumentException>(() => MediaAssetTranslation.NormalizeAltText("  "));
    }

    private static async Task<MediaAsset> SeedAssetAsync(MediaDbContext db, CancellationToken ct)
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        var asset = MediaAsset.Create("image/png", 10, now, status: MediaAssetStatus.Ready);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);
        return asset;
    }

    private static MediaDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new MediaDbContext(options);
    }
}
