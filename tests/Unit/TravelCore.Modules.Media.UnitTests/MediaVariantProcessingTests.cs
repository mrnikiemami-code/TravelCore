using Microsoft.EntityFrameworkCore;
using NodaTime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Media.Infrastructure.Processing;
using TravelCore.Modules.Media.Infrastructure.Services;
using TravelCore.Modules.Media.Infrastructure.Storage;
using TravelCore.Modules.Media.Contracts;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

public sealed class MediaVariantProcessingTests
{
    [Fact]
    public async Task GenerateForAsset_CreatesReadyAndNotRequired_WithoutTouchingOriginalReady()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var storage = new InMemoryMediaObjectStorage();
        var processor = new ImageSharpMediaVariantProcessor();
        var service = new MediaVariantApplicationService(db, storage, processor, SystemClock.Instance);

        var png = CreatePngBytes(2000, 1500);
        const string key = "2026/08/16/original-demo.png";
        await using (var put = new MemoryStream(png))
        {
            await storage.PutAsync(new MediaObjectPutRequest(key, put, "image/png", png.LongLength), ct);
        }

        var now = SystemClock.Instance.GetCurrentInstant();
        var asset = MediaAsset.Create(
            "image/png",
            png.LongLength,
            now,
            storageKey: key,
            status: MediaAssetStatus.Ready);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);

        var variants = await service.GenerateForAssetAsync(asset.Id.Value, ct);
        Assert.Equal(3, variants.Count);

        var large = Assert.Single(variants, v => v.Profile == "Large");
        Assert.Equal("Ready", large.Status);
        Assert.Equal(1600, large.Width);
        Assert.Equal(1200, large.Height);
        Assert.False(string.IsNullOrWhiteSpace(large.StorageKey));
        Assert.Equal("image/png", large.ContentType);
        Assert.True(await storage.ExistsAsync(large.StorageKey!, ct));

        var medium = Assert.Single(variants, v => v.Profile == "Medium");
        Assert.Equal("Ready", medium.Status);
        Assert.Equal(960, medium.Width);
        Assert.Equal(720, medium.Height);

        var thumb = Assert.Single(variants, v => v.Profile == "Thumbnail");
        Assert.Equal("Ready", thumb.Status);
        Assert.Equal(320, thumb.Width);
        Assert.Equal(240, thumb.Height);

        var reloaded = await db.MediaAssets.SingleAsync(x => x.Id == asset.Id, ct);
        Assert.Equal(MediaAssetStatus.Ready, reloaded.Status);
        Assert.Equal(key, reloaded.StorageKey);
        Assert.Equal(2000, reloaded.Width);
        Assert.Equal(1500, reloaded.Height);

        // Idempotent regenerate
        var again = await service.GenerateForAssetAsync(asset.Id.Value, ct);
        Assert.Equal(3, again.Count);
        Assert.Equal(3, await db.MediaVariants.CountAsync(ct));
    }

    [Fact]
    public async Task GenerateForAsset_SmallSource_MarksLargeAndMediumNotRequired()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var storage = new InMemoryMediaObjectStorage();
        var service = new MediaVariantApplicationService(
            db,
            storage,
            new ImageSharpMediaVariantProcessor(),
            SystemClock.Instance);

        var png = CreatePngBytes(700, 500);
        const string key = "2026/08/16/small.png";
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

        var variants = await service.GenerateForAssetAsync(asset.Id.Value, ct);
        Assert.Equal("NotRequired", Assert.Single(variants, v => v.Profile == "Large").Status);
        Assert.Equal("NotRequired", Assert.Single(variants, v => v.Profile == "Medium").Status);
        var thumb = Assert.Single(variants, v => v.Profile == "Thumbnail");
        Assert.Equal("Ready", thumb.Status);
        Assert.Equal(320, thumb.Width);
        Assert.Equal(229, thumb.Height);
        Assert.Null(Assert.Single(variants, v => v.Profile == "Large").StorageKey);
    }

    [Fact]
    public async Task GenerateForAsset_Gif_FailsClosed_LeavesOriginalReady()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var storage = new InMemoryMediaObjectStorage();
        var service = new MediaVariantApplicationService(
            db,
            storage,
            new ImageSharpMediaVariantProcessor(),
            SystemClock.Instance);

        var gif = "GIF89a"u8.ToArray();
        const string key = "2026/08/16/anim.gif";
        await using (var put = new MemoryStream(gif))
        {
            await storage.PutAsync(new MediaObjectPutRequest(key, put, "image/gif", gif.LongLength), ct);
        }

        var asset = MediaAsset.Create(
            "image/gif",
            gif.LongLength,
            SystemClock.Instance.GetCurrentInstant(),
            storageKey: key,
            status: MediaAssetStatus.Ready);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GenerateForAssetAsync(asset.Id.Value, ct));
        Assert.Contains("GIF variant policy is unresolved", ex.Message, StringComparison.Ordinal);

        var reloaded = await db.MediaAssets.SingleAsync(x => x.Id == asset.Id, ct);
        Assert.Equal(MediaAssetStatus.Ready, reloaded.Status);
        Assert.Empty(await db.MediaVariants.ToListAsync(ct));
    }

    [Fact]
    public void EnsureSupportedOutputFormat_RejectsGif()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ImageSharpMediaVariantProcessor.EnsureSupportedOutputFormat("image/gif"));
        Assert.Contains("GIF variant policy is unresolved", ex.Message, StringComparison.Ordinal);
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
