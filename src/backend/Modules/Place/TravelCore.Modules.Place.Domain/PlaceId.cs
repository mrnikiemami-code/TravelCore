using TravelCore.Identifiers;

namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Strongly typed Place identity (UUID v7).
/// Canonical catalog id for Hotel / Restaurant / Attraction (P07-R1) — no independent public subtype ids.
/// </summary>
public readonly record struct PlaceId(Guid Value) : IEquatable<PlaceId>
{
    public static PlaceId New() => new(Uuid7.New());

    public static PlaceId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(value));
        }

        return new PlaceId(value);
    }

    public override string ToString() => Value.ToString("D");
}
