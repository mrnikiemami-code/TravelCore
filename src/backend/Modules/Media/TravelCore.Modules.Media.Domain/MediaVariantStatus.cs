namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Processing posture for a derived MediaVariant. Failed variants must not invalidate original Ready.
/// </summary>
public enum MediaVariantStatus : short
{
    Ready = 1,
    Failed = 2,
    /// <summary>Source already fits within the profile max; no derived blob is stored.</summary>
    NotRequired = 3
}
