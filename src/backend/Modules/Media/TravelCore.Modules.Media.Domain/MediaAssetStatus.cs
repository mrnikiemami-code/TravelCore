namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Technical processing posture for a MediaAsset. Not a consumer publication/SEO flag.
/// </summary>
public enum MediaAssetStatus : short
{
    /// <summary>Metadata registered; binary not yet bound/ready (T003/T004 complete the path).</summary>
    PendingStorage = 0,

    /// <summary>Technical metadata is consistent with a usable storage binding.</summary>
    Ready = 1,

    /// <summary>Processing or validation failed; asset is not deliverable.</summary>
    Failed = 2,
}
