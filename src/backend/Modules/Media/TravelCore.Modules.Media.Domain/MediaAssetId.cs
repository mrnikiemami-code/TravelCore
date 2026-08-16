using TravelCore.Identifiers;

namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Strongly typed MediaAsset identity (UUID v7).
/// </summary>
public readonly record struct MediaAssetId(Guid Value) : IEquatable<MediaAssetId>
{
    public static MediaAssetId New() => new(Uuid7.New());

    public static MediaAssetId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(value));
        }

        return new MediaAssetId(value);
    }

    public override string ToString() => Value.ToString("D");
}
