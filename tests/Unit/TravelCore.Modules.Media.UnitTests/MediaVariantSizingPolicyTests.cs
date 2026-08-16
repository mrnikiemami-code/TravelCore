using TravelCore.Modules.Media.Domain;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

public sealed class MediaVariantSizingPolicyTests
{
    [Fact]
    public void FitWithin_4000x3000_Large_Is_1600x1200()
    {
        var (w, h) = MediaVariantSizingPolicy.FitWithinProfile(4000, 3000, MediaVariantProfile.Large);
        Assert.Equal(1600, w);
        Assert.Equal(1200, h);
    }

    [Fact]
    public void FitWithin_4000x3000_Medium_Is_960x720()
    {
        var (w, h) = MediaVariantSizingPolicy.FitWithinProfile(4000, 3000, MediaVariantProfile.Medium);
        Assert.Equal(960, w);
        Assert.Equal(720, h);
    }

    [Fact]
    public void FitWithin_4000x3000_Thumbnail_Is_320x240()
    {
        var (w, h) = MediaVariantSizingPolicy.FitWithinProfile(4000, 3000, MediaVariantProfile.Thumbnail);
        Assert.Equal(320, w);
        Assert.Equal(240, h);
    }

    [Fact]
    public void FitWithin_DoesNotUpscale_700x500()
    {
        var (w, h) = MediaVariantSizingPolicy.FitWithin(700, 500, MediaVariantSizingPolicy.LargeMaxLongestEdge);
        Assert.Equal(700, w);
        Assert.Equal(500, h);
        Assert.True(MediaVariantSizingPolicy.IsNotRequired(700, 500, MediaVariantProfile.Large));
        Assert.True(MediaVariantSizingPolicy.IsNotRequired(700, 500, MediaVariantProfile.Medium));
        Assert.False(MediaVariantSizingPolicy.IsNotRequired(700, 500, MediaVariantProfile.Thumbnail));
    }

    [Fact]
    public void EnsureWithinDecodeLimits_RejectsOversizedEdge()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            MediaVariantSizingPolicy.EnsureWithinDecodeLimits(
                MediaVariantSizingPolicy.MaxDecodeWidth + 1,
                100));
        Assert.Contains("max edge", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureWithinDecodeLimits_RejectsPixelBudget()
    {
        // 8000x5001 = 40_008_000 > 40_000_000
        var ex = Assert.Throws<ArgumentException>(() =>
            MediaVariantSizingPolicy.EnsureWithinDecodeLimits(8000, 5001));
        Assert.Contains("max pixel", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureWithinDecodeLimits_AllowsBoundary()
    {
        MediaVariantSizingPolicy.EnsureWithinDecodeLimits(8000, 5000);
        MediaVariantSizingPolicy.EnsureWithinDecodeLimits(12000, 1);
    }
}
