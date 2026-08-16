using Microsoft.EntityFrameworkCore;
using NodaTime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Media.Infrastructure.Processing;
using TravelCore.Modules.Media.Infrastructure.Services;
using TravelCore.Modules.Media.Infrastructure.Storage;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

public sealed class MediaContentDeliveryTests
{
    [Fact]
    public async Task OpenOriginal_Ready_StreamsBytes_WithTrustedContentType()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var storage = new InMemoryMediaObjectStorage();
        var delivery = new MediaContentDeliveryService(db, storage);

        var png = CreatePngBytes(32, 24);
        const string key = "2026/08/16/ready-original.png";
        await using (var put = new MemoryStream(png))
        {
            await storage.PutAsync(new MediaObjectPutRequest(key, put, "image/png", png.LongLength), ct);
        }

        var asset = MediaAsset.Create(
            "image/png",
            png.LongLength,
            SystemClock.Instance.GetCurrentInstant(),
            width: 32,
            height: 24,
            storageKey: key,
            status: MediaAssetStatus.Ready);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);

        await using var opened = await delivery.OpenOriginalAsync(asset.Id.Value, ct);
        Assert.NotNull(opened);
        Assert.Equal("image/png", opened!.ContentType);
        Assert.Equal("original", opened.Representation);
        Assert.Equal(asset.Id.Value, opened.MediaAssetId);
        using var ms = new MemoryStream();
        await opened.Content.CopyToAsync(ms, ct);
        Assert.Equal(png, ms.ToArray());
    }

    [Fact]
    public async Task OpenOriginal_PendingOrFailed_ReturnsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var storage = new InMemoryMediaObjectStorage();
        var delivery = new MediaContentDeliveryService(db, storage);
        var now = SystemClock.Instance.GetCurrentInstant();

        var pending = MediaAsset.Create("image/png", 10, now, status: MediaAssetStatus.PendingStorage);
        var failed = MediaAsset.Create("image/png", 10, now, storageKey: "x", status: MediaAssetStatus.Failed);
        db.MediaAssets.AddRange(pending, failed);
        await db.SaveChangesAsync(ct);

        Assert.Null(await delivery.OpenOriginalAsync(pending.Id.Value, ct));
        Assert.Null(await delivery.OpenOriginalAsync(failed.Id.Value, ct));
    }

    [Fact]
    public async Task OpenVariant_Ready_Streams_And_NotRequiredReturnsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var storage = new InMemoryMediaObjectStorage();
        var processor = new ImageSharpMediaVariantProcessor();
        var variants = new MediaVariantApplicationService(db, storage, processor, SystemClock.Instance);
        var delivery = new MediaContentDeliveryService(db, storage);

        var png = CreatePngBytes(2000, 1500);
        const string key = "2026/08/16/delivery-source.png";
        await using (var put = new MemoryStream(png))
        {
            await storage.PutAsync(new MediaObjectPutRequest(key, put, "image/png", png.LongLength), ct);
        }

        var asset = MediaAsset.Create(
            "image/png",
            png.LongLength,
            SystemClock.Instance.GetCurrentInstant(),
            storageKey: key,
            status: MediaAssetStatus.Ready);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);

        await variants.GenerateForAssetAsync(asset.Id.Value, ct);

        await using var large = await delivery.OpenVariantAsync(asset.Id.Value, "large", ct);
        Assert.NotNull(large);
        Assert.Equal("image/png", large!.ContentType);
        Assert.Equal("large", large.Representation);
        Assert.True(large.ContentLength > 0);

        // Case-insensitive profile segment
        await using var medium = await delivery.OpenVariantAsync(asset.Id.Value, "Medium", ct);
        Assert.NotNull(medium);

        // Unknown profile
        Assert.Null(await delivery.OpenVariantAsync(asset.Id.Value, "hero", ct));
    }

    [Fact]
    public async Task OpenVariant_NotRequired_ReturnsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var storage = new InMemoryMediaObjectStorage();
        var variants = new MediaVariantApplicationService(
            db,
            storage,
            new ImageSharpMediaVariantProcessor(),
            SystemClock.Instance);
        var delivery = new MediaContentDeliveryService(db, storage);

        var png = CreatePngBytes(700, 500);
        const string key = "2026/08/16/small-delivery.png";
        await using (var put = new MemoryStream(png))
        {
            await storage.PutAsync(new MediaObjectPutRequest(key, put, "image/png", png.LongLength), ct);
        }

        var asset = MediaAsset.Create(
            "image/png",
            png.LongLength,
            SystemClock.Instance.GetCurrentInstant(),
            storageKey: key,
            status: MediaAssetStatus.Ready);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);
        await variants.GenerateForAssetAsync(asset.Id.Value, ct);

        Assert.Null(await delivery.OpenVariantAsync(asset.Id.Value, "large", ct));
        Assert.Null(await delivery.OpenVariantAsync(asset.Id.Value, "medium", ct));
        Assert.NotNull(await delivery.OpenVariantAsync(asset.Id.Value, "thumbnail", ct));
    }

    [Fact]
    public void PresentationUrls_NeverEmbedStorageKey()
    {
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var original = MediaPresentationUrls.OriginalContent(id);
        var variant = MediaPresentationUrls.VariantContent(id, "Large");

        Assert.Equal($"/api/media/assets/{id:D}/content", original);
        Assert.Equal($"/api/media/assets/{id:D}/variants/large/content", variant);
        Assert.DoesNotContain("storage", original, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("storage", variant, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("bucket", variant, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Presentation_MapsAppProxyUrls_WithoutStorageKey()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var storage = new InMemoryMediaObjectStorage();
        var translations = new MediaAssetTranslationApplicationService(db, SystemClock.Instance);
        var presentation = new MediaPresentationApplicationService(db, translations);
        var variants = new MediaVariantApplicationService(
            db,
            storage,
            new ImageSharpMediaVariantProcessor(),
            SystemClock.Instance);

        var png = CreatePngBytes(2000, 1500);
        const string key = "secret/provider/path/never-public.png";
        await using (var put = new MemoryStream(png))
        {
            await storage.PutAsync(new MediaObjectPutRequest(key, put, "image/png", png.LongLength), ct);
        }

        var asset = MediaAsset.Create(
            "image/png",
            png.LongLength,
            SystemClock.Instance.GetCurrentInstant(),
            width: 2000,
            height: 1500,
            storageKey: key,
            status: MediaAssetStatus.Ready);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);
        await variants.GenerateForAssetAsync(asset.Id.Value, ct);

        var dto = await presentation.GetPresentationAsync(asset.Id.Value, localeCode: null, ct);
        Assert.NotNull(dto);
        Assert.Equal("Ready", dto!.Status);
        Assert.Equal(MediaPresentationUrls.OriginalContent(asset.Id.Value), dto.OriginalContentUrl);
        Assert.DoesNotContain(key, dto.OriginalContentUrl!, StringComparison.Ordinal);
        Assert.All(dto.Variants, v =>
        {
            Assert.DoesNotContain(key, v.ContentUrl ?? string.Empty, StringComparison.Ordinal);
            if (v.Status == "Ready")
            {
                Assert.False(string.IsNullOrWhiteSpace(v.ContentUrl));
                Assert.Contains("/variants/", v.ContentUrl!, StringComparison.Ordinal);
                Assert.EndsWith("/content", v.ContentUrl!, StringComparison.Ordinal);
            }
        });
    }

    private static MediaDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new MediaDbContext(options);
    }

    private static byte[] CreatePngBytes(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }
}
