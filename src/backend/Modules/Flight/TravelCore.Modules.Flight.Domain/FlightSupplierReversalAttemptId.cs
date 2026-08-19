using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Strongly typed FlightSupplierReversalAttempt identity (UUID v7).
/// </summary>
public readonly record struct FlightSupplierReversalAttemptId(Guid Value)
{
    public static FlightSupplierReversalAttemptId New() => new(Uuid7.New());

    public static FlightSupplierReversalAttemptId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightSupplierReversalAttemptId cannot be empty.", nameof(value));
        }

        return new FlightSupplierReversalAttemptId(value);
    }

    public override string ToString() => Value.ToString("D");
}
