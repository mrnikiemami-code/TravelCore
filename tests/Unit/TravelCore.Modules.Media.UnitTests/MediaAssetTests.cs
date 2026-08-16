using NodaTime;
using TravelCore.Modules.Media.Domain;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

public sealed class MediaAssetTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 8, 30);

    [Fact]
    public void Create_NormalizesMime_And_PreservesMetadata()
    {
        var asset = MediaAsset.Create(
            " Image/JPEG ",
            1024,
            Now,
            width: 800,
            height: 600,
            storageKey: " originals/2026/asset-1.jpg ",
            status: MediaAssetStatus.Ready);

        Assert.Equal("image/jpeg", asset.ContentType);
        Assert.Equal(1024, asset.ByteSize);
        Assert.Equal(800, asset.Width);
        Assert.Equal(600, asset.Height);
        Assert.Equal("originals/2026/asset-1.jpg", asset.StorageKey);
        Assert.Equal(MediaAssetStatus.Ready, asset.Status);
        Assert.Equal(Now, asset.CreatedAt);
        Assert.Equal(Now, asset.UpdatedAt);
        Assert.NotEqual(Guid.Empty, asset.Id.Value);
    }

    [Fact]
    public void Create_AllowsPendingWithoutStorageKey()
    {
        var asset = MediaAsset.Create("image/png", 10, Now);
        Assert.Equal(MediaAssetStatus.PendingStorage, asset.Status);
        Assert.Null(asset.StorageKey);
        Assert.Null(asset.Width);
        Assert.Null(asset.Height);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("image")]
    public void Create_RejectsInvalidContentType(string contentType)
    {
        Assert.ThrowsAny<ArgumentException>(() => MediaAsset.Create(contentType, 1, Now));
    }

    [Fact]
    public void Create_RejectsNegativeByteSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MediaAsset.Create("image/png", -1, Now));
    }

    [Fact]
    public void Create_RejectsNonPositiveDimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MediaAsset.Create("image/png", 1, Now, width: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MediaAsset.Create("image/png", 1, Now, height: -5));
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("a\\b")]
    [InlineData("/absolute")]
    public void Create_RejectsUnsafeStorageKeys(string key)
    {
        Assert.Throws<ArgumentException>(() =>
            MediaAsset.Create("image/png", 1, Now, storageKey: key));
    }

    [Fact]
    public void MediaAssetId_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => MediaAssetId.From(Guid.Empty));
    }
}
