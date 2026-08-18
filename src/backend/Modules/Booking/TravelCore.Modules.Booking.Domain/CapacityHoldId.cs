using TravelCore.Identifiers;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Strongly typed CapacityHold identity (UUID v7). Not a client-supplied hold token (P19-R3).
/// </summary>
public readonly record struct CapacityHoldId(Guid Value)
{
    public static CapacityHoldId New() => new(Uuid7.New());

    public static CapacityHoldId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CapacityHoldId cannot be empty.", nameof(value));
        }

        return new CapacityHoldId(value);
    }

    public override string ToString() => Value.ToString("D");
}
