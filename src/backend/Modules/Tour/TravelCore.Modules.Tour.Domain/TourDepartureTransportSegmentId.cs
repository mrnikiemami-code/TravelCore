using TravelCore.Identifiers;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Strongly typed identity for <see cref="TourDepartureTransportSegment"/> (UUID v7).
/// </summary>
public readonly record struct TourDepartureTransportSegmentId(Guid Value)
    : IEquatable<TourDepartureTransportSegmentId>
{
    public static TourDepartureTransportSegmentId New() => new(Uuid7.New());

    public static TourDepartureTransportSegmentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TourDepartureTransportSegmentId cannot be empty.", nameof(value));
        }

        return new TourDepartureTransportSegmentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
