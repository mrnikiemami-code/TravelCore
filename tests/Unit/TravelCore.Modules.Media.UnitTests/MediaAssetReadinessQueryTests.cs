using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Media.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

/// <summary>
/// TC-P07-T005: smallest Media Ready probe for Place attach validation.
/// </summary>
public sealed class MediaAssetReadinessQueryTests
{
    [Fact]
    public async Task IsReadyAsync_EmptyGuid_ReturnsFalse()
    {
        await using var db = CreateDb();
        var query = new MediaAssetReadinessQuery(db);
        Assert.False(await query.IsReadyAsync(Guid.Empty));
    }

    [Fact]
    public void MediaAssetStatus_ReadyIsDistinctFromPendingAndFailed()
    {
        Assert.Equal(MediaAssetStatus.Ready, (MediaAssetStatus)1);
        Assert.NotEqual(MediaAssetStatus.PendingStorage, MediaAssetStatus.Ready);
        Assert.NotEqual(MediaAssetStatus.Failed, MediaAssetStatus.Ready);
        Assert.Equal(
            "TravelCore.Modules.Media.Contracts",
            typeof(IMediaAssetReadinessQuery).Namespace);
    }

    private static MediaDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase($"media-ready-{Guid.NewGuid():N}")
            .Options;
        return new MediaDbContext(options);
    }
}
