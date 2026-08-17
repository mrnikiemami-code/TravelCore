using TravelCore.Identifiers;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Strongly typed identity for <see cref="TourDepartureAccommodationOption"/> (UUID v7).
/// </summary>
public readonly record struct TourDepartureAccommodationOptionId(Guid Value)
    : IEquatable<TourDepartureAccommodationOptionId>
{
    public static TourDepartureAccommodationOptionId New() => new(Uuid7.New());

    public static TourDepartureAccommodationOptionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TourDepartureAccommodationOptionId cannot be empty.", nameof(value));
        }

        return new TourDepartureAccommodationOptionId(value);
    }

    public override string ToString() => Value.ToString("D");
}
