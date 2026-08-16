using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Media.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

public sealed class MediaFocalPointServiceTests
{
    [Fact]
    public async Task Set_Then_Get_PersistsAndReturnsFocalPoint()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var service = new MediaFocalPointApplicationService(db, SystemClock.Instance);
        var assets = new MediaAssetApplicationService(db, SystemClock.Instance);

        var now = SystemClock.Instance.GetCurrentInstant();
        var asset = MediaAsset.Create("image/png", 100, now, status: MediaAssetStatus.Ready);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);

        var set = await service.SetAsync(
            asset.Id.Value,
            new UpsertFocalPointRequest(0.2, 0.8),
            ct);
        Assert.Equal(asset.Id.Value, set.MediaAssetId);
        Assert.Equal(0.2, set.FocalX);
        Assert.Equal(0.8, set.FocalY);

        var got = await service.GetAsync(asset.Id.Value, ct);
        Assert.NotNull(got);
        Assert.Equal(0.2, got.FocalX);
        Assert.Equal(0.8, got.FocalY);

        var read = await assets.GetByIdAsync(asset.Id.Value, ct);
        Assert.NotNull(read);
        Assert.Equal(0.2, read.FocalX);
        Assert.Equal(0.8, read.FocalY);
    }

    [Fact]
    public async Task Set_MissingAsset_Throws()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var service = new MediaFocalPointApplicationService(db, SystemClock.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SetAsync(
                Guid.Parse("22222222-2222-7222-8222-222222222222"),
                new UpsertFocalPointRequest(0.5, 0.5),
                ct));
    }

    [Fact]
    public async Task Set_Clear_RemovesCoordinates()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateDb();
        var service = new MediaFocalPointApplicationService(db, SystemClock.Instance);

        var now = SystemClock.Instance.GetCurrentInstant();
        var asset = MediaAsset.Create("image/jpeg", 50, now);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync(ct);

        await service.SetAsync(asset.Id.Value, new UpsertFocalPointRequest(0.1, 0.9), ct);
        var cleared = await service.SetAsync(
            asset.Id.Value,
            new UpsertFocalPointRequest(null, null),
            ct);

        Assert.Null(cleared.FocalX);
        Assert.Null(cleared.FocalY);
    }

    private static MediaDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new MediaDbContext(options);
    }
}
