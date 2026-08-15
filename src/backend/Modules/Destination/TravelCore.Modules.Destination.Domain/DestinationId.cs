using TravelCore.Identifiers;

namespace TravelCore.Modules.Destination.Domain;

/// <summary>
/// Strongly typed Destination identity (UUID v7).
/// </summary>
public readonly record struct DestinationId(Guid Value) : IEquatable<DestinationId>
{
    public static DestinationId New() => new(Uuid7.New());

    public static DestinationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(value));
        }

        return new DestinationId(value);
    }

    public override string ToString() => Value.ToString("D");
}
