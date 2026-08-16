using TravelCore.Identifiers;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Strongly typed TourProduct identity (UUID v7).
/// Canonical Tour product id (P09-R1) — Experience/Package have no independent public ids.
/// </summary>
public readonly record struct TourProductId(Guid Value) : IEquatable<TourProductId>
{
    public static TourProductId New() => new(Uuid7.New());

    public static TourProductId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(value));
        }

        return new TourProductId(value);
    }

    public override string ToString() => Value.ToString("D");
}
