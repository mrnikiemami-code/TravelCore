namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Fit-within sizing for derived variants (P06-R3 sizing lock).
/// Preserves aspect; no crop; no upscale. Decode limits reject oversized sources.
/// </summary>
public static class MediaVariantSizingPolicy
{
    public const int LargeMaxLongestEdge = 1600;
    public const int MediumMaxLongestEdge = 960;
    public const int ThumbnailMaxLongestEdge = 320;

    public const int MaxDecodeWidth = 12000;
    public const int MaxDecodeHeight = 12000;
    public const long MaxDecodePixels = 40_000_000L;

    public static int MaxLongestEdgeFor(MediaVariantProfile profile) =>
        profile switch
        {
            MediaVariantProfile.Large => LargeMaxLongestEdge,
            MediaVariantProfile.Medium => MediumMaxLongestEdge,
            MediaVariantProfile.Thumbnail => ThumbnailMaxLongestEdge,
            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unsupported MediaVariantProfile.")
        };

    public static IReadOnlyList<MediaVariantProfile> AllDerivedProfiles { get; } =
    [
        MediaVariantProfile.Large,
        MediaVariantProfile.Medium,
        MediaVariantProfile.Thumbnail
    ];

    /// <summary>
    /// Rejects decode when either edge or pixel count exceeds locked limits.
    /// Does not mutate MediaAsset status — caller must leave original Ready alone.
    /// </summary>
    public static void EnsureWithinDecodeLimits(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                $"Decoded dimensions must be positive (got {width}x{height}).");
        }

        if (width > MaxDecodeWidth || height > MaxDecodeHeight)
        {
            throw new ArgumentException(
                $"Decoded image exceeds max edge limit {MaxDecodeWidth}x{MaxDecodeHeight} (got {width}x{height}).");
        }

        var pixels = (long)width * height;
        if (pixels > MaxDecodePixels)
        {
            throw new ArgumentException(
                $"Decoded image exceeds max pixel limit {MaxDecodePixels} (got {pixels} for {width}x{height}).");
        }
    }

    /// <summary>
    /// True when the source already fits the profile max longest edge (no derived blob).
    /// </summary>
    public static bool IsNotRequired(int sourceWidth, int sourceHeight, MediaVariantProfile profile)
    {
        EnsurePositiveSource(sourceWidth, sourceHeight);
        var longest = Math.Max(sourceWidth, sourceHeight);
        return longest <= MaxLongestEdgeFor(profile);
    }

    /// <summary>
    /// Computes target size fit-within max longest edge. Never upscales; never crops.
    /// </summary>
    public static (int Width, int Height) FitWithin(int sourceWidth, int sourceHeight, int maxLongestEdge)
    {
        EnsurePositiveSource(sourceWidth, sourceHeight);
        if (maxLongestEdge <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLongestEdge), maxLongestEdge, "Max longest edge must be positive.");
        }

        var longest = Math.Max(sourceWidth, sourceHeight);
        if (longest <= maxLongestEdge)
        {
            return (sourceWidth, sourceHeight);
        }

        var scale = (double)maxLongestEdge / longest;
        var width = Math.Max(1, (int)Math.Round(sourceWidth * scale, MidpointRounding.AwayFromZero));
        var height = Math.Max(1, (int)Math.Round(sourceHeight * scale, MidpointRounding.AwayFromZero));
        return (width, height);
    }

    public static (int Width, int Height) FitWithinProfile(
        int sourceWidth,
        int sourceHeight,
        MediaVariantProfile profile) =>
        FitWithin(sourceWidth, sourceHeight, MaxLongestEdgeFor(profile));

    private static void EnsurePositiveSource(int sourceWidth, int sourceHeight)
    {
        if (sourceWidth <= 0 || sourceHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceWidth),
                $"Source dimensions must be positive (got {sourceWidth}x{sourceHeight}).");
        }
    }
}
