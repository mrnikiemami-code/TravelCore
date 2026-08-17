using TravelCore.Identifiers;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Strongly typed TripIntent identity (UUID v7).
/// </summary>
public readonly record struct TripIntentId(Guid Value) : IEquatable<TripIntentId>
{
    public static TripIntentId New() => new(Uuid7.New());

    public static TripIntentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TripIntentId cannot be empty.", nameof(value));
        }

        return new TripIntentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
