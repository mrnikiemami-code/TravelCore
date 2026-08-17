using TravelCore.Identifiers;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Strongly typed TourDeparture identity (UUID v7). Distinct from <see cref="TourProductId"/> (P11-R1).
/// </summary>
public readonly record struct TourDepartureId(Guid Value) : IEquatable<TourDepartureId>
{
    public static TourDepartureId New() => new(Uuid7.New());

    public static TourDepartureId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TourDepartureId cannot be empty.", nameof(value));
        }

        return new TourDepartureId(value);
    }

    public override string ToString() => Value.ToString("D");
}
