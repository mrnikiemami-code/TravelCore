namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Derived variant profiles. Original is the logical source MediaAsset — not a stored duplicate row.
/// </summary>
public enum MediaVariantProfile : short
{
    Large = 1,
    Medium = 2,
    Thumbnail = 3
}
