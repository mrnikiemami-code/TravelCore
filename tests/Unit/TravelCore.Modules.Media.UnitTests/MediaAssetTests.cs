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

    [Fact]
    public void SetFocalPoint_StoresNormalizedCoordinates()
    {
        var asset = MediaAsset.Create("image/png", 10, Now);
        var later = Instant.FromUtc(2026, 8, 16, 9, 0);

        asset.SetFocalPoint(0.25, 0.75, later);

        Assert.Equal(0.25, asset.FocalX);
        Assert.Equal(0.75, asset.FocalY);
        Assert.Equal(later, asset.UpdatedAt);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(0.5, 0.5)]
    public void SetFocalPoint_AcceptsInclusiveBounds(double x, double y)
    {
        var asset = MediaAsset.Create("image/png", 10, Now);
        asset.SetFocalPoint(x, y, Now);
        Assert.Equal(x, asset.FocalX);
        Assert.Equal(y, asset.FocalY);
    }

    [Theory]
    [InlineData(-0.01, 0.5)]
    [InlineData(0.5, 1.01)]
    [InlineData(double.NaN, 0.5)]
    [InlineData(0.5, double.PositiveInfinity)]
    public void SetFocalPoint_RejectsOutOfRange(double x, double y)
    {
        var asset = MediaAsset.Create("image/png", 10, Now);
        Assert.Throws<ArgumentOutOfRangeException>(() => asset.SetFocalPoint(x, y, Now));
    }

    [Fact]
    public void SetFocalPoint_RejectsPartialPair()
    {
        var asset = MediaAsset.Create("image/png", 10, Now);
        Assert.Throws<ArgumentException>(() => asset.SetFocalPoint(0.5, null, Now));
        Assert.Throws<ArgumentException>(() => asset.SetFocalPoint(null, 0.5, Now));
    }

    [Fact]
    public void SetFocalPoint_ClearsWhenBothNull()
    {
        var asset = MediaAsset.Create("image/png", 10, Now);
        asset.SetFocalPoint(0.3, 0.4, Now);
        var later = Instant.FromUtc(2026, 8, 16, 10, 0);

        asset.SetFocalPoint(null, null, later);

        Assert.Null(asset.FocalX);
        Assert.Null(asset.FocalY);
        Assert.Equal(later, asset.UpdatedAt);
    }
}
