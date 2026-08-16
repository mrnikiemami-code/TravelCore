using TravelCore.Identifiers;

namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Strongly typed MediaVariant identity (UUID v7).
/// </summary>
public readonly record struct MediaVariantId(Guid Value) : IEquatable<MediaVariantId>
{
    public static MediaVariantId New() => new(Uuid7.New());

    public static MediaVariantId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("MediaVariantId cannot be empty.", nameof(value));
        }

        return new MediaVariantId(value);
    }

    public override string ToString() => Value.ToString("D");
}
